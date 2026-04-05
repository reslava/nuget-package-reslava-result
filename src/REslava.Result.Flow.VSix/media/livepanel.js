// Live Panel webview script.
// Init data is passed via window.__livePanel__ (set by a tiny inline script before this loads).
(async function () {
  let baseDiagram       = window.__livePanel__.diagram;
  let currentMethodName = window.__livePanel__.method;

  const vscode = acquireVsCodeApi();

  // ── State ──────────────────────────────────────────────────────────────────
  let allTraces     = [];
  let mode          = 'history';
  let selectedTrace = null;
  let nodeIdx       = 0;
  let replayTimer   = null;
  let hasFileData   = false;  // true once file-based traces received; ignores pollError after that
  let dataSource    = 'waiting';  // 'waiting' | 'file' | 'http'

  // ── DOM refs ───────────────────────────────────────────────────────────────
  const hdrName      = document.getElementById('hdr-name');
  const statusDot    = document.getElementById('status-dot');
  const sourceBadge  = document.getElementById('source-badge');
  const hintBar      = document.getElementById('hint-bar');
  const hintMsg      = document.getElementById('hint-msg');
  const tracePanel   = document.getElementById('trace-panel');
  const traceList    = document.getElementById('trace-list');
  const emptyMsg     = document.getElementById('empty-msg');
  const stepperPanel = document.getElementById('stepper-panel');
  const stepperTitle = document.getElementById('stepper-title');
  const nodeListEl   = document.getElementById('node-list');
  const btnPrev      = document.getElementById('btn-prev');
  const btnNext      = document.getElementById('btn-next');
  const btnPlay      = document.getElementById('btn-play');
  const btnPause     = document.getElementById('btn-pause');
  const replayBar    = document.getElementById('replay-bar');
  const replayFill   = document.getElementById('replay-fill');
  const diagramArea  = document.getElementById('diagram-area');
  const diagramEl    = document.getElementById('diagram-el');
  const fileBar      = document.getElementById('file-bar');
  const filePicker   = document.getElementById('file-picker');

  mermaid.initialize({ startOnLoad: false, securityLevel: 'loose',
                       flowchart: { useMaxWidth: true } });

  // ── File picker ────────────────────────────────────────────────────────────
  filePicker.addEventListener('change', () => {
    vscode.postMessage({ command: 'loadFile', path: filePicker.value });
  });

  // ── Mode buttons (History | Step only) ────────────────────────────────────
  const modeBtns = {
    history: document.getElementById('btn-history'),
    step:    document.getElementById('btn-step'),
  };
  Object.entries(modeBtns).forEach(([m, btn]) =>
    btn.addEventListener('click', () => setMode(m)));

  function setMode(m) {
    if (m === mode) { return; }
    stopReplay();
    mode = m;
    Object.entries(modeBtns).forEach(([k, btn]) =>
      btn.classList.toggle('active', k === m));
    if (m === 'step' && !selectedTrace) {
      if (allTraces.length > 0) { selectedTrace = allTraces[allTraces.length - 1]; nodeIdx = 0; }
    }
    render();
  }

  // ── Hint copy ──────────────────────────────────────────────────────────────
  document.getElementById('btn-hint-copy').addEventListener('click', () => {
    navigator.clipboard.writeText(
      'PipelineTraceHost.Start(buffer) or app.MapResultFlowTraces(buffer)');
  });

  // ── Back button ────────────────────────────────────────────────────────────
  document.getElementById('btn-back').addEventListener('click', () => {
    stopReplay();
    selectedTrace = null;
    nodeIdx = 0;
    setMode('history');
  });

  // ── Stepper nav ────────────────────────────────────────────────────────────
  btnPrev.addEventListener('click', async () => {
    if (nodeIdx > 0) { nodeIdx--; await renderStepper(); }
  });
  btnNext.addEventListener('click', async () => {
    if (selectedTrace && nodeIdx < selectedTrace.nodes.length - 1) {
      nodeIdx++; await renderStepper();
    }
  });
  btnPlay.addEventListener('click',  startReplay);
  btnPause.addEventListener('click', stopReplay);

  // ── Messages from extension host ───────────────────────────────────────────
  // Polling is done in the extension host (Node.js) to avoid webview sandbox
  // restrictions. The host pushes { command: 'traces', traces } on each poll.
  window.addEventListener('message', async e => {
    const msg = e.data;
    if (msg.command === 'status') {
      dataSource = msg.source;
      if (msg.source === 'file') {
        sourceBadge.textContent = '\uD83D\uDCC4 file' + (msg.detail ? ' \u00B7 ' + msg.detail : '');
        sourceBadge.title = 'Traces loaded from reslava-traces.json';
        sourceBadge.className = 'source-badge source-file';
      } else if (msg.source === 'http') {
        sourceBadge.textContent = '\uD83C\uDF10 http';
        sourceBadge.title = 'Traces from HTTP endpoint';
        sourceBadge.className = 'source-badge source-http';
      } else {
        sourceBadge.textContent = '\u23F3 waiting';
        sourceBadge.className = 'source-badge source-waiting';
      }
    } else if (msg.command === 'traces') {
      hasFileData = dataSource === 'file';
      allTraces = msg.traces;
      statusDot.className = 'status-dot ok';
      statusDot.title = 'Connected';
      hintBar.style.display = 'none';
      if (dataSource === 'http') {
        sourceBadge.textContent = '\uD83C\uDF10 http';
        sourceBadge.className = 'source-badge source-http';
      }
      await render();
    } else if (msg.command === 'pollError') {
      if (hasFileData) {
        // File data takes precedence — show HTTP badge as inactive but keep traces
        sourceBadge.title = 'HTTP endpoint not reachable — showing file data';
        return;
      }
      statusDot.className = 'status-dot err';
      statusDot.title = msg.message;
      sourceBadge.textContent = '\uD83C\uDF10 \u2717';
      sourceBadge.className = 'source-badge source-err';
      hintBar.style.display = '';
      hintMsg.textContent = msg.message + ' \u2014 start the trace endpoint:';
      if (mode === 'history' || mode === 'single') { renderTraceList([]); }
    } else if (msg.command === 'setFileList') {
      if (msg.files.length <= 1) { fileBar.style.display = 'none'; return; }
      fileBar.style.display = '';
      filePicker.innerHTML = msg.files.map(f =>
        '<option value="' + escAttr(f.path) + '"' +
        (f.path === msg.selectedPath ? ' selected' : '') + '>' +
        escHtml(f.label) + '</option>'
      ).join('');
    } else if (msg.command === 'setMethod') {
      currentMethodName = msg.methodName;
      hdrName.textContent = '\u25BA ' + msg.methodName;
      if (msg.diagram !== undefined) {
        baseDiagram = msg.diagram;
        if (mode === 'step' || mode === 'replay') {
          const nodeId = selectedTrace?.nodes[nodeIdx]?.nodeId ?? null;
          await renderHighlightedDiagram(nodeId);
        }
      }
    }
  });

  // ── Render dispatch ────────────────────────────────────────────────────────
  async function render() {
    if (mode === 'step' || mode === 'replay') {
      showStepper();
      await renderStepper();
    } else {
      showTraceList();
      renderTraceList(visibleTraces());
    }
  }

  function visibleTraces() {
    return allTraces;
  }

  // ── Trace list ─────────────────────────────────────────────────────────────
  function showTraceList() {
    tracePanel.style.display = '';
    stepperPanel.style.display = 'none';
    replayBar.style.display = 'none';
  }

  function renderTraceList(traces) {
    if (traces.length === 0) {
      emptyMsg.style.display = '';
      traceList.innerHTML = '';
      traceList.appendChild(emptyMsg);
      return;
    }
    emptyMsg.style.display = 'none';
    traceList.innerHTML = traces.map((t, i) => {
      const icon    = t.isSuccess ? '&#10003;' : '&#10005;';
      const iconClr = t.isSuccess ? '#22c55e' : '#ef4444';
      const errPart = t.errorType ? ' &middot; ' + escHtml(t.errorType) : '';
      const age     = relativeTime(t.endedAt);
      const nCount  = (t.nodes?.length ?? 0) + ' nodes';
      return '<div class="trace-row" data-idx="' + i + '">' +
        '<span class="trace-idx">#' + (i + 1) + '</span>' +
        '<span class="trace-icon" style="color:' + iconClr + '">' + icon + '</span>' +
        '<span class="trace-name">' + escHtml(t.methodName) + '</span>' +
        '<span class="trace-meta">' + t.elapsedMs + 'ms &middot; ' +
          nCount + errPart + ' &middot; ' + age + '</span>' +
        '</div>';
    }).join('');

    traceList.querySelectorAll('.trace-row').forEach(row => {
      row.addEventListener('click', () => {
        const idx = parseInt(row.getAttribute('data-idx'), 10);
        selectedTrace = allTraces[idx];
        nodeIdx = 0;
        stopReplay();
        setMode('step');
      });
    });
  }

  // ── Stepper ────────────────────────────────────────────────────────────────
  function showStepper() {
    tracePanel.style.display = 'none';
    stepperPanel.style.display = '';
  }

  async function renderStepper() {
    if (!selectedTrace) { return; }
    const nodes = selectedTrace.nodes ?? [];
    const total = nodes.length;

    const inputPart = selectedTrace.inputValue ? '  \u00b7  Input: ' + selectedTrace.inputValue : '';
    stepperTitle.textContent =
      'Node ' + (nodeIdx + 1) + ' of ' + total +
      '  \u2014  ' + selectedTrace.methodName +
      (selectedTrace.isSuccess ? '  \u2713' : '  \u2717 ' + (selectedTrace.errorType ?? '')) +
      inputPart;

    btnPrev.disabled = (nodeIdx === 0);
    btnNext.disabled = (nodeIdx >= total - 1);

    if (mode === 'replay' && replayTimer !== null) {
      btnPlay.style.display = 'none'; btnPause.style.display = '';
    } else {
      btnPlay.style.display = ''; btnPause.style.display = 'none';
    }

    replayBar.style.display = mode === 'replay' ? '' : 'none';
    replayFill.style.width =
      (total > 1 ? (nodeIdx / (total - 1)) * 100 : 100) + '%';

    nodeListEl.innerHTML = nodes.map((n, i) => {
      const isCur  = (i === nodeIdx);
      const icon   = n.isSuccess
        ? '<span style="color:#22c55e">&#10003;</span>'
        : '<span style="color:#ef4444">&#10005;</span>';
      const outLine = n.outputValue
        ? '<div class="node-output">' + escHtml(truncate(n.outputValue, 60)) + '</div>'
        : n.errorType
          ? '<div class="node-output" style="color:#ef4444">' + escHtml(n.errorType) + '</div>'
          : '';
      return '<div class="node-row' + (isCur ? ' current' : '') + '">' +
        '<span class="node-idx">' + (i + 1) + '</span>' +
        '<div class="node-cell">' +
          '<span class="node-name">' + escHtml(n.stepName) + '</span>' + outLine +
        '</div>' +
        '<div class="node-meta">' + icon + ' ' + n.elapsedMs + 'ms</div>' +
        '</div>';
    }).join('');

    const currentRow = nodeListEl.querySelector('.node-row.current');
    if (currentRow) { currentRow.scrollIntoView({ block: 'nearest' }); }

    // ── Diagram node highlight ────────────────────────────────────────────────
    await renderHighlightedDiagram(nodes[nodeIdx]?.nodeId ?? null);
  }

  // ── Diagram node highlight ─────────────────────────────────────────────────
  async function renderHighlightedDiagram(nodeId) {
    if (!baseDiagram) { diagramArea.style.display = 'none'; return; }
    diagramArea.style.display = '';

    // Remove any previously injected highlight lines, then re-apply
    let d = baseDiagram
      .replace(/\nclass\s+\w+\s+__rrf_hl/g, '')
      .replace(/\nclassDef\s+__rrf_hl[^\n]*/g, '');

    if (nodeId) {
      d += '\nclassDef __rrf_hl fill:#ffe066,stroke:#f59e0b,stroke-width:2.5px';
      d += '\nclass ' + nodeId + ' __rrf_hl';
    }

    try {
      diagramEl.removeAttribute('data-processed');
      diagramEl.textContent = d;
      await mermaid.run({ nodes: [diagramEl] });
    } catch (_) {
      // Mermaid parse error — hide diagram rather than crash
      diagramArea.style.display = 'none';
    }
  }

  // ── Replay ─────────────────────────────────────────────────────────────────
  function startReplay() {
    if (!selectedTrace) { return; }
    stopReplay();
    nodeIdx = 0;
    renderStepper();
    replayTimer = setInterval(async () => {
      const total = selectedTrace ? selectedTrace.nodes.length : 0;
      if (nodeIdx < total - 1) {
        nodeIdx++;
        await renderStepper();
      } else {
        stopReplay();
        await renderStepper();   // swap Pause → Play
      }
    }, 800);
    btnPlay.style.display = 'none'; btnPause.style.display = '';
    replayBar.style.display = '';
  }

  function stopReplay() {
    if (replayTimer !== null) { clearInterval(replayTimer); replayTimer = null; }
  }

  // ── Utilities ──────────────────────────────────────────────────────────────
  function escAttr(s) {
    return String(s).replace(/&/g, '&amp;').replace(/"/g, '&quot;');
  }
  function escHtml(s) {
    if (!s) { return ''; }
    return String(s)
      .replace(/&/g, '&amp;').replace(/</g, '&lt;')
      .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  }
  function truncate(s, n) {
    return s && s.length > n ? s.slice(0, n) + '\u2026' : s;
  }
  function relativeTime(iso) {
    if (!iso) { return ''; }
    const diff = (Date.now() - new Date(iso).getTime()) / 1000;
    if (diff < 5)    { return 'just now'; }
    if (diff < 60)   { return Math.round(diff) + 's ago'; }
    if (diff < 3600) { return Math.round(diff / 60) + 'm ago'; }
    return Math.round(diff / 3600) + 'h ago';
  }
})();
