import * as vscode from 'vscode';

// Multiple mode: one panel per method, keyed by methodName.
const panels = new Map<string, vscode.WebviewPanel>();
// Single mode: one shared panel for all methods.
let singlePanel: vscode.WebviewPanel | undefined;

function getMode(): 'single' | 'multiple' {
    return vscode.workspace.getConfiguration('reslava')
        .get<string>('diagramWindowMode', 'single') === 'multiple' ? 'multiple' : 'single';
}

// Sends a message to all currently open panels (used to sync mode button label on toggle).
export function notifyAllPanels(message: object): void {
    singlePanel?.webview.postMessage(message);
    for (const p of panels.values()) { p.webview.postMessage(message); }
}

// Creates or reveals the WebviewPanel for the given method.
export function showWebviewPanel(
    methodName: string,
    diagram: string,
    extensionUri: vscode.Uri,
    typeFlow:         string | null = null,
    layerView:        string | null = null,
    stats:            string | null = null,
    errorSurface:     string | null = null,
    errorPropagation: string | null = null
): void {
    const mode = getMode();
    const extra = { typeFlow, layerView, stats, errorSurface, errorPropagation };

    if (mode === 'single') {
        if (singlePanel) {
            singlePanel.title = `Pipeline: ${methodName}`;
            singlePanel.reveal(singlePanel.viewColumn ?? vscode.ViewColumn.Beside);
            singlePanel.webview.postMessage({ command: 'update', diagram, methodName, ...extra });
            return;
        }
        singlePanel = createPanel(methodName, diagram, extensionUri, mode, extra);
        singlePanel.onDidDispose(() => { singlePanel = undefined; });
        return;
    }

    // Multiple mode
    const existing = panels.get(methodName);
    if (existing) {
        existing.reveal(existing.viewColumn ?? vscode.ViewColumn.Beside);
        existing.webview.postMessage({ command: 'update', diagram, ...extra });
        return;
    }
    const panel = createPanel(methodName, diagram, extensionUri, mode, extra);
    panel.onDidDispose(() => panels.delete(methodName));
    panels.set(methodName, panel);
}

interface ExtraViews {
    typeFlow?:         string | null;
    layerView?:        string | null;
    stats?:            string | null;
    errorSurface?:     string | null;
    errorPropagation?: string | null;
}

function createPanel(
    methodName: string,
    diagram: string,
    extensionUri: vscode.Uri,
    mode: 'single' | 'multiple',
    extra: ExtraViews = {}
): vscode.WebviewPanel {
    const panel = vscode.window.createWebviewPanel(
        'reslava.resultFlow',
        `Pipeline: ${methodName}`,
        vscode.ViewColumn.Beside,
        {
            enableScripts: true,
            localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'media')],
            retainContextWhenHidden: true
        }
    );
    panel.webview.html = buildHtml(diagram, methodName, panel.webview, extensionUri, mode, extra);
    panel.webview.onDidReceiveMessage(async (msg) => {
        if (msg.command === 'navigate') {
            await navigateToVscodeUri(msg.href);
        } else if (msg.command === 'exportSvg') {
            await exportFile(methodName, msg.data, 'svg');
        } else if (msg.command === 'exportPng') {
            await exportFile(methodName, msg.data, 'png');
        } else if (msg.command === 'pngError') {
            vscode.window.showErrorMessage(`PNG export failed: ${msg.message}`);
        } else if (msg.command === 'toggleWindowMode') {
            await vscode.commands.executeCommand('reslava.toggleDiagramWindowMode');
        }
    });
    return panel;
}

// Posts a diagram update to an already-open panel (used by auto-refresh on save).
// No-op if the panel is not open.
export function refreshWebviewPanel(
    methodName: string,
    diagram: string,
    typeFlow:         string | null = null,
    layerView:        string | null = null,
    stats:            string | null = null,
    errorSurface:     string | null = null,
    errorPropagation: string | null = null
): void {
    const msg = { command: 'update', diagram, typeFlow, layerView, stats, errorSurface, errorPropagation };
    singlePanel?.webview.postMessage(msg);
    panels.get(methodName)?.webview.postMessage(msg);
}

// Returns true if a panel for the given method is currently open.
export function isPanelOpen(methodName: string): boolean {
    return panels.has(methodName);
}

// Returns 'dark' if the diagram's %%{init}%% contains themeVariables, otherwise 'light'.
// The generator always uses theme:'base'; dark mode is identified by the presence of
// themeVariables (primaryTextColor, edgeLabelBackground, etc.) in the init directive.
function detectTheme(diagram: string): 'light' | 'dark' {
    return /themeVariables/.test(diagram) ? 'dark' : 'light';
}

function buildHtml(
    diagram: string,
    methodName: string,
    webview: vscode.Webview,
    extensionUri: vscode.Uri,
    mode: 'single' | 'multiple' = 'single',
    extra: ExtraViews = {}
): string {
    const { typeFlow = null, layerView = null, stats = null,
            errorSurface = null, errorPropagation = null } = extra;
    const mermaidUri = webview.asWebviewUri(
        vscode.Uri.joinPath(extensionUri, 'media', 'mermaid.min.js')
    );
    const nonce = getNonce();
    // JSON.stringify escapes both strings safely for embedding in JavaScript.
    const diagramJson      = JSON.stringify(diagram);
    const typeFlowJson     = JSON.stringify(typeFlow);
    const layerViewJson    = JSON.stringify(layerView);
    const statsJson        = JSON.stringify(stats);
    const errorSurfaceJson = JSON.stringify(errorSurface);
    const errorPropJson    = JSON.stringify(errorPropagation);

    const isDark = detectTheme(diagram) === 'dark';
    // Page background follows ResultFlowDefaultTheme — white for light, dark for dark.
    const pageBg     = isDark ? '#1e1e1e' : '#ffffff';
    const textColor  = isDark ? '#d4d4d4' : '#333333';
    const headColor  = isDark ? '#9cdcfe' : '#555555';
    const brandColor = isDark ? '#555555' : '#bbbbbb';
    const panelBg    = isDark ? '#252525' : '#f8f8f8';
    const panelBdr   = isDark ? '#3c3c3c' : '#e0e0e0';
    const btnBg      = isDark ? '#2e2e2e' : '#f0f0f0';
    const btnBdr     = isDark ? '#444444' : '#cccccc';
    const btnColor   = isDark ? '#bbbbbb' : '#555555';
    const hintColor  = isDark ? '#888888' : '#999999';

    // Node palette — matched exactly to ResultFlowThemes.cs Light/Dark classDef
    const P = isDark ? {
        op: ['#3a2b1f','#f2c2a0',''],         bi: ['#1f3a2d','#9fe0c0','#2d5a4a'],
        ma: ['#1f3a2d','#9fe0c0',''],          gk: ['#1f263a','#b8c8f2',''],
        se: ['#3a331f','#f2e0a0',''],          te: ['#33203a','#d6b0f2',''],
        fa: ['#3a1f1f','#f2b8b8',''],
    } : {
        op: ['#faf0e3','#b45f2b',''],          bi: ['#e3f0e8','#2f7a5c','#1a5c3c'],
        ma: ['#e3f0e8','#2f7a5c',''],          gk: ['#e3e9fa','#3f5c9a',''],
        se: ['#fff4d9','#b8882c',''],          te: ['#f2e3f5','#8a4f9e',''],
        fa: ['#f8e3e3','#b13e3e',''],
    };
    const sw = (p: string[]) =>
        `<span class="sw" style="background:${p[0]};color:${p[1]};${p[2] ? `border:2px solid ${p[2]};` : ''}"></span>`;

    return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <meta http-equiv="Content-Security-Policy"
        content="default-src 'none';
                 script-src 'nonce-${nonce}' ${webview.cspSource};
                 style-src 'unsafe-inline';
                 img-src data:;
                 clipboard-write 'self';" />
  <title>Pipeline: ${methodName}</title>
  <script nonce="${nonce}" src="${mermaidUri}"></script>
  <style>
    * { box-sizing: border-box; }
    body { background:${pageBg}; color:${textColor}; margin:0; padding:12px 16px;
           font-family:sans-serif; }

    /* ── Header ── */
    .header { display:flex; align-items:baseline; justify-content:space-between;
              margin-bottom:6px; gap:8px; }
    .method-name { color:${headColor}; font-size:14px; font-weight:400;
                   letter-spacing:0.4px; white-space:nowrap; overflow:hidden;
                   text-overflow:ellipsis; }
    .brand { color:${brandColor}; font-size:10px; white-space:nowrap; flex-shrink:0; }

    /* ── Toolbar ── */
    .toolbar { display:flex; gap:4px; margin-bottom:10px; }
    .tb-btn {
      background:${btnBg}; color:${btnColor}; border:1px solid ${btnBdr};
      border-radius:3px; padding:2px 10px; cursor:pointer;
      font-size:11px; font-family:sans-serif; user-select:none;
    }
    .tb-btn:hover { opacity:0.8; }
    .tb-btn.active { background:${isDark ? '#3a3a3a' : '#e0e8f0'}; }

    /* ── Diagram ── */
    #diagram-scroll { overflow:auto; width:100%; }
    .mermaid { display:block; }
    .mermaid svg { max-width:100%; height:auto; display:block; }

    /* ── Collapsible panels ── */
    .cpanel { display:none; margin-top:12px; border:1px solid ${panelBdr};
              border-radius:4px; background:${panelBg}; padding:10px 12px; }

    /* ── Source panel ── */
    .source-bar { display:flex; gap:6px; align-items:center; margin-bottom:6px; }
    .src-btn {
      background:${btnBg}; color:${btnColor}; border:1px solid ${btnBdr};
      border-radius:3px; padding:1px 8px; cursor:pointer;
      font-size:11px; font-family:sans-serif; user-select:none;
    }
    .src-btn:hover { opacity:0.8; }
    .copy-ok { color:${isDark ? '#6dbf67' : '#2e7d32'}; font-size:11px; }
    #source-pre {
      background:${isDark ? '#1a1a1a' : '#ffffff'}; color:${textColor};
      border:1px solid ${panelBdr}; border-radius:3px; padding:8px 10px;
      font-size:11px; font-family:monospace;
      white-space:pre-wrap; word-break:break-all;
      max-height:260px; overflow:auto; margin:0;
    }

    /* ── Legend panel ── */
    .legend-grid { display:grid; grid-template-columns:20px 1fr; gap:4px 8px;
                   align-items:center; margin-bottom:10px; }
    .sw { display:inline-block; width:16px; height:16px; border-radius:3px;
          border:1px solid transparent; flex-shrink:0; }
    .legend-label { font-size:12px; }
    .legend-hints { border-top:1px solid ${panelBdr}; padding-top:8px;
                    font-size:11px; color:${hintColor}; line-height:1.8; }
  </style>
</head>
<body>
  <div class="header">
    <span class="method-name">&#9654; ${methodName}</span>
    <span class="brand">REslava.Result Extensions</span>
  </div>
  <div class="toolbar">
    <button class="tb-btn" id="btn-source">Source</button>
    <button class="tb-btn" id="btn-legend">Legend</button>
    <button class="tb-btn" id="btn-types"     title="Type-flow view"${typeFlow     ? '' : ' disabled style="opacity:0.4;cursor:default"'}>Types</button>
    <button class="tb-btn" id="btn-layer"     title="Layer view"${layerView        ? '' : ' disabled style="opacity:0.4;cursor:default"'}>Layer</button>
    <button class="tb-btn" id="btn-errors"    title="Error surface"${errorSurface  ? '' : ' disabled style="opacity:0.4;cursor:default"'}>Errors</button>
    <button class="tb-btn" id="btn-errorprop" title="Error propagation"${errorPropagation ? '' : ' disabled style="opacity:0.4;cursor:default"'}>Prop</button>
    <button class="tb-btn" id="btn-stats"     title="Pipeline statistics"${stats   ? '' : ' disabled style="opacity:0.4;cursor:default"'}>Stats</button>
    <button class="tb-btn" id="btn-svg"    title="Export as SVG">SVG</button>
    <button class="tb-btn" id="btn-png"    title="Export as PNG (2x)">PNG</button>
    <button class="tb-btn" id="btn-mode"   title="Toggle single/multiple window mode">${mode === 'single' ? 'Single' : 'Multi'}</button>
  </div>
  <div id="diagram-scroll">
    <div class="mermaid"></div>
  </div>

  <div id="source-panel" class="cpanel">
    <div class="source-bar">
      <button class="src-btn" id="btn-copy">Copy</button>
      <span id="copy-ok" class="copy-ok" style="display:none">&#10003; Copied</span>
    </div>
    <pre id="source-pre"></pre>
  </div>

  <div id="stats-panel" class="cpanel">
    <pre id="stats-pre" style="font-size:12px;font-family:monospace;white-space:pre-wrap;margin:0"></pre>
  </div>

  <div id="legend-panel" class="cpanel">
    <div class="legend-grid">
      ${sw(P.op)}<span class="legend-label">Root — pipeline entry point</span>
      ${sw(P.bi)}<span class="legend-label">Bind — transform with possible error</span>
      ${sw(P.ma)}<span class="legend-label">Map — pure transform</span>
      ${sw(P.gk)}<span class="legend-label">Gatekeeper — Ensure / Filter</span>
      ${sw(P.se)}<span class="legend-label">Tap — side effect (logging, events)</span>
      ${sw(P.te)}<span class="legend-label">Terminal — Match (final routing) &#x2B21;</span>
      ${sw(P.fa)}<span class="legend-label">Failure — error sink</span>
    </div>
    <div class="legend-hints">
      &#x1F5B1; <b>Hover</b> a Gatekeeper node to see its predicate<br>
      &#x1F5B1; <b>Click</b> any node to navigate to that line in source<br>
      &#x26A1; = async &nbsp;|&nbsp; FAIL: 1&#x2013;3 errors inline &nbsp;|&nbsp; 4+ as &#x2139;&#xFE0F; tooltip
    </div>
    <div class="legend-hints" style="margin-top:6px">
      <b>Constants:</b> _Diagram &nbsp;&#xB7;&nbsp; _TypeFlow &nbsp;&#xB7;&nbsp; _LayerView &nbsp;&#xB7;&nbsp; _Stats &nbsp;&#xB7;&nbsp; _ErrorSurface &nbsp;&#xB7;&nbsp; _ErrorPropagation
    </div>
  </div>

  <script nonce="${nonce}">
  (async function () {
    let diagram      = ${diagramJson};
    let typeFlow     = ${typeFlowJson};
    let layerView    = ${layerViewJson};
    let stats        = ${statsJson};
    let errorSurface = ${errorSurfaceJson};
    let errorProp    = ${errorPropJson};
    let currentView  = 'diagram'; // 'diagram' | 'typeflow' | 'layer' | 'errors' | 'errorprop'

    const el      = document.querySelector('.mermaid');
    const vscode  = acquireVsCodeApi();

    mermaid.initialize({ startOnLoad: false, securityLevel: 'loose',
                         flowchart: { useMaxWidth: true } });

    async function renderDiagram(d) {
      el.removeAttribute('data-processed');
      el.textContent = d;
      document.getElementById('source-pre').textContent = d;
      await mermaid.run({ nodes: [el] });
    }

    // ── Toolbar panel toggles ────────────────────────────────────────────────
    function togglePanel(panelId, btnId) {
      const panel = document.getElementById(panelId);
      const btn   = document.getElementById(btnId);
      const shown = getComputedStyle(panel).display !== 'none';
      panel.style.display = shown ? 'none' : 'block';
      btn.classList.toggle('active', !shown);
    }
    document.getElementById('btn-source').addEventListener('click', () =>
      togglePanel('source-panel', 'btn-source'));
    document.getElementById('btn-legend').addEventListener('click', () =>
      togglePanel('legend-panel', 'btn-legend'));
    document.getElementById('btn-stats').addEventListener('click', () => {
      if (!stats) { return; }
      document.getElementById('stats-pre').textContent = stats;
      togglePanel('stats-panel', 'btn-stats');
    });

    // ── Diagram view switching (radio: only one active at a time) ────────────
    const VIEW_MAP = {
      'btn-types':     () => typeFlow,
      'btn-layer':     () => layerView,
      'btn-errors':    () => errorSurface,
      'btn-errorprop': () => errorProp,
    };
    const VIEW_KEYS = {
      'btn-types': 'typeflow', 'btn-layer': 'layer',
      'btn-errors': 'errors',  'btn-errorprop': 'errorprop'
    };

    function clearViewBtns() {
      Object.keys(VIEW_MAP).forEach(id => document.getElementById(id)?.classList.remove('active'));
    }

    async function activateView(btnId) {
      const content = VIEW_MAP[btnId]?.();
      if (!content) { return; }
      const viewKey = VIEW_KEYS[btnId];
      if (currentView === viewKey) {
        // toggle off → back to main diagram
        currentView = 'diagram';
        clearViewBtns();
        await renderDiagram(diagram);
      } else {
        currentView = viewKey;
        clearViewBtns();
        document.getElementById(btnId).classList.add('active');
        await renderDiagram(content);
      }
    }

    Object.keys(VIEW_MAP).forEach(btnId => {
      document.getElementById(btnId)?.addEventListener('click', () => activateView(btnId));
    });

    // ── Copy source ───────────────────────────────────────────────────────────
    document.getElementById('btn-copy').addEventListener('click', () => {
      navigator.clipboard.writeText(document.getElementById('source-pre').textContent || '')
        .then(() => {
          const ok = document.getElementById('copy-ok');
          ok.style.display = 'inline';
          setTimeout(() => { ok.style.display = 'none'; }, 1500);
        });
    });

    // ── vscode:// click interception ─────────────────────────────────────────
    document.addEventListener('click', e => {
      const a = e.target.closest('a');
      if (!a) { return; }
      const href = a.getAttribute('href')
                || a.getAttributeNS('http://www.w3.org/1999/xlink', 'href');
      if (href && href.startsWith('vscode://')) {
        e.preventDefault();
        vscode.postMessage({ command: 'navigate', href });
      }
    });

    // ── Export ────────────────────────────────────────────────────────────────
    function getSvgString() {
      const svg = el.querySelector('svg');
      if (!svg) { return null; }
      return new XMLSerializer().serializeToString(svg);
    }

    document.getElementById('btn-svg').addEventListener('click', () => {
      const svgStr = getSvgString();
      if (svgStr) { vscode.postMessage({ command: 'exportSvg', data: svgStr }); }
    });

    document.getElementById('btn-png').addEventListener('click', () => {
      const svgEl = el.querySelector('svg');
      if (!svgEl) { return; }
      const rect  = svgEl.getBoundingClientRect();
      const w = rect.width  || 800;
      const h = rect.height || 600;
      const scale = 2; // 2x for high-DPI PNG

      // Clone and stamp explicit px dimensions so the browser can size the image.
      // Mermaid SVGs often omit these, causing Image to load at 0x0.
      const clone = svgEl.cloneNode(true);
      clone.setAttribute('width',  w);
      clone.setAttribute('height', h);
      const svgStr = new XMLSerializer().serializeToString(clone);

      // base64 is more reliable than encodeURIComponent for large SVGs
      const b64 = btoa(unescape(encodeURIComponent(svgStr)));
      const img = new Image();
      img.onload = () => {
        try {
          const canvas = document.createElement('canvas');
          canvas.width  = w * scale;
          canvas.height = h * scale;
          const ctx = canvas.getContext('2d');
          ctx.scale(scale, scale);
          ctx.fillStyle = '${pageBg}';
          ctx.fillRect(0, 0, w, h);
          ctx.drawImage(img, 0, 0, w, h);
          const dataUrl = canvas.toDataURL('image/png');
          vscode.postMessage({ command: 'exportPng', data: dataUrl });
        } catch (err) {
          vscode.postMessage({ command: 'pngError', message: String(err) });
        }
      };
      img.onerror = (e) => {
        vscode.postMessage({ command: 'pngError', message: 'img.onerror fired: ' + String(e) });
      };
      img.src = 'data:image/svg+xml;base64,' + b64;
    });

    // ── Mode toggle ──────────────────────────────────────────────────────────
    document.getElementById('btn-mode').addEventListener('click', () => {
      vscode.postMessage({ command: 'toggleWindowMode' });
    });

    // ── Plan #4 auto-refresh + mode sync ─────────────────────────────────────
    window.addEventListener('message', async event => {
      const msg = event.data;
      if (msg.command === 'update') {
        if (msg.methodName) {
          document.querySelector('.method-name').textContent = '▶ ' + msg.methodName;
        }
        // Reset to main diagram view and update all constants
        currentView      = 'diagram';
        diagram          = msg.diagram;
        typeFlow         = msg.typeFlow         ?? null;
        layerView        = msg.layerView        ?? null;
        stats            = msg.stats            ?? null;
        errorSurface     = msg.errorSurface     ?? null;
        errorProp        = msg.errorPropagation ?? null;
        clearViewBtns();
        // Sync disabled states for view buttons
        const viewBtnStates = {
          'btn-types': typeFlow, 'btn-layer': layerView,
          'btn-errors': errorSurface, 'btn-errorprop': errorProp, 'btn-stats': stats
        };
        Object.entries(viewBtnStates).forEach(([id, val]) => {
          const b = document.getElementById(id);
          if (b) { b.disabled = !val; b.style.opacity = val ? '' : '0.4'; b.style.cursor = val ? '' : 'default'; }
        });
        await renderDiagram(diagram);
      } else if (msg.command === 'windowModeChanged') {
        document.getElementById('btn-mode').textContent = msg.mode === 'single' ? 'Single' : 'Multi';
      }
    });

    await renderDiagram(diagram);
  })();
  </script>
</body>
</html>`;
}

function getNonce(): string {
    let text = '';
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    for (let i = 0; i < 32; i++) {
        text += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return text;
}

// Parses vscode://file/{absolutePath}:{1-based-line}  or  ...:{line}:{col}
// and navigates to that position in the editor.
// Windows paths contain a colon (C:/...) — the suffix is stripped from the right.
async function navigateToVscodeUri(href: string): Promise<void> {
    if (!href.startsWith('vscode://file/')) { return; }

    const pathAndPos = href.slice('vscode://file/'.length);

    // Strip trailing numeric suffixes from the right to isolate the file path.
    // e.g. "C:/path/file.cs:42:0"  → filePath="C:/path/file.cs", line=42, col=0
    //      "C:/path/file.cs:42"    → filePath="C:/path/file.cs", line=42, col=0
    const colMatch = pathAndPos.match(/:(\d+)$/);
    if (!colMatch) { return; }

    const withoutLast = pathAndPos.slice(0, pathAndPos.length - colMatch[0].length);
    const lineMatch   = withoutLast.match(/:(\d+)$/);

    let filePath: string;
    let line: number;
    let col: number;

    if (lineMatch) {
        // Three-part: path:line:col
        filePath = withoutLast.slice(0, withoutLast.length - lineMatch[0].length);
        line     = Math.max(0, parseInt(lineMatch[1], 10) - 1);
        col      = Math.max(0, parseInt(colMatch[1],  10) - 1);
    } else {
        // Two-part: path:line
        filePath = withoutLast;
        line     = Math.max(0, parseInt(colMatch[1], 10) - 1);
        col      = 0;
    }

    if (!filePath) { return; }

    try {
        const uri = vscode.Uri.file(filePath);
        const doc = await vscode.workspace.openTextDocument(uri);
        await vscode.window.showTextDocument(doc, {
            viewColumn: vscode.ViewColumn.One,
            selection: new vscode.Range(line, col, line, col)
        });
    } catch {
        // File may not exist or may have moved — silently ignore
    }
}

// Shows a save dialog and writes the exported diagram to disk.
// For PNG, data is a base64 data URL ("data:image/png;base64,...").
// For SVG, data is the raw SVG XML string.
async function exportFile(methodName: string, data: string, ext: 'svg' | 'png'): Promise<void> {
    const wsFolder = vscode.workspace.workspaceFolders?.[0]?.uri;
    const defaultUri = wsFolder
        ? vscode.Uri.joinPath(wsFolder, `${methodName}.ResultFlow.${ext}`)
        : vscode.Uri.file(`${methodName}.ResultFlow.${ext}`);

    const saveUri = await vscode.window.showSaveDialog({
        defaultUri,
        filters: ext === 'svg' ? { 'SVG Image': ['svg'] } : { 'PNG Image': ['png'] },
        title: `Export Pipeline Diagram — ${methodName}`
    });
    if (!saveUri) { return; }

    const bytes = ext === 'svg'
        ? Buffer.from(data, 'utf8')
        : Buffer.from(data.split(',')[1] ?? '', 'base64');

    await vscode.workspace.fs.writeFile(saveUri, bytes);
    vscode.window.showInformationMessage(`Exported: ${saveUri.fsPath}`);
}
