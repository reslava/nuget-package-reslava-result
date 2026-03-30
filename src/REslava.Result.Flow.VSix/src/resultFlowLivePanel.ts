import * as vscode from 'vscode';
import * as http from 'http';
import { findDiagramByMethodName } from './diagramResolver';

export class ResultFlowLivePanel {
    static readonly viewType = 'resultflow.livePanel';
    private static _current: ResultFlowLivePanel | undefined;

    private readonly _panel: vscode.WebviewPanel;
    private _pollTimer: ReturnType<typeof setInterval> | undefined;
    private readonly _port: number;
    private readonly _pollInterval: number;
    private readonly _log: vscode.OutputChannel;

    /** Opens or reveals the Live panel for the given method. */
    static show(extensionUri: vscode.Uri, methodName: string, log: vscode.OutputChannel): void {
        const diagram = findDiagramByMethodName(methodName);  // null if not built yet

        if (ResultFlowLivePanel._current) {
            ResultFlowLivePanel._current._panel.reveal(vscode.ViewColumn.Beside);
            ResultFlowLivePanel._current._panel.title = `Live: ${methodName}`;
            ResultFlowLivePanel._current._panel.webview.postMessage({
                command: 'setMethod', methodName, diagram
            });
            return;
        }

        const panel = vscode.window.createWebviewPanel(
            ResultFlowLivePanel.viewType,
            `Live: ${methodName}`,
            vscode.ViewColumn.Beside,
            {
                enableScripts: true,
                retainContextWhenHidden: true,
                localResourceRoots: [vscode.Uri.joinPath(extensionUri, 'media')]
            }
        );

        ResultFlowLivePanel._current = new ResultFlowLivePanel(panel, extensionUri, methodName, diagram, log);
    }

    private constructor(
        panel: vscode.WebviewPanel,
        extensionUri: vscode.Uri,
        methodName: string,
        diagram: string | null,
        log: vscode.OutputChannel
    ) {
        const config = vscode.workspace.getConfiguration();
        this._port         = config.get<number>('resultflow.tracePort',          5297);
        this._pollInterval = config.get<number>('resultflow.tracePollIntervalMs', 2000);
        this._log          = log;

        this._panel = panel;
        this._panel.webview.html = buildLivePanelHtml(methodName, diagram, extensionUri, panel.webview);
        this._panel.onDidDispose(() => {
            this._stopPolling();
            ResultFlowLivePanel._current = undefined;
        });
        this._log.appendLine(`[LivePanel] opened for "${methodName}" — port ${this._port}, poll ${this._pollInterval}ms`);
        this._startPolling();
    }

    // ── Extension-host polling ─────────────────────────────────────────────────
    // Polls the trace endpoint from Node.js (no browser sandbox restrictions)
    // and pushes results to the webview via postMessage.

    private _startPolling(): void {
        const url = `http://localhost:${this._port}/reslava/traces`;
        this._log.appendLine(`[LivePanel] polling ${url} every ${this._pollInterval}ms`);

        const doPoll = () => {
            this._log.appendLine(`[LivePanel] → GET ${url}`);
            const req = http.get(url, (res) => {
                let raw = '';
                res.on('data', (chunk: Buffer) => { raw += chunk.toString(); });
                res.on('end', () => {
                    this._log.appendLine(`[LivePanel] ← HTTP ${res.statusCode}  body: ${raw.slice(0, 120)}`);
                    if (res.statusCode === 200) {
                        try {
                            const traces = JSON.parse(raw);
                            this._log.appendLine(`[LivePanel] parsed ${traces.length} trace(s) — posting to webview`);
                            const sent = this._panel.webview.postMessage({ command: 'traces', traces });
                            sent.then(ok => this._log.appendLine(`[LivePanel] postMessage delivered: ${ok}`));
                        } catch (err) {
                            this._log.appendLine(`[LivePanel] JSON parse error: ${err}`);
                            this._panel.webview.postMessage({ command: 'pollError', message: 'JSON parse error' });
                        }
                    } else {
                        this._log.appendLine(`[LivePanel] unexpected status ${res.statusCode}`);
                        this._panel.webview.postMessage({
                            command: 'pollError',
                            message: `Endpoint returned HTTP ${res.statusCode}`
                        });
                    }
                });
            });
            req.on('error', (e: Error) => {
                this._log.appendLine(`[LivePanel] request error: ${e.message}`);
                this._panel.webview.postMessage({ command: 'pollError', message: e.message });
            });
        };

        // Delay the first poll slightly to let the webview finish loading
        setTimeout(doPoll, 500);
        this._pollTimer = setInterval(doPoll, this._pollInterval);
    }

    private _stopPolling(): void {
        if (this._pollTimer !== undefined) {
            clearInterval(this._pollTimer);
            this._pollTimer = undefined;
        }
    }
}

// ─── HTML builder ─────────────────────────────────────────────────────────────

function buildLivePanelHtml(
    methodName: string,
    diagram: string | null,
    extensionUri: vscode.Uri,
    webview: vscode.Webview
): string {
    const nonce        = getNonce();
    const mermaidUri   = webview.asWebviewUri(
        vscode.Uri.joinPath(extensionUri, 'media', 'mermaid.min.js'));
    const scriptUri    = webview.asWebviewUri(
        vscode.Uri.joinPath(extensionUri, 'media', 'livepanel.js'));

    const methodJson  = JSON.stringify(methodName);
    const diagramJson = JSON.stringify(diagram);

    return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <meta http-equiv="Content-Security-Policy"
        content="default-src 'none';
                 script-src 'nonce-${nonce}' ${webview.cspSource};
                 style-src 'unsafe-inline';
                 img-src data:;" />
  <title>Live Panel</title>
  <script nonce="${nonce}" src="${mermaidUri}"></script>
  <style>
    *, *::before, *::after { box-sizing: border-box; }
    body {
      margin: 0; padding: 10px 14px;
      background: var(--vscode-editor-background);
      color: var(--vscode-editor-foreground);
      font-family: var(--vscode-font-family, sans-serif);
      font-size: 13px;
    }

    /* ── Header ── */
    .hdr {
      display: flex; align-items: center; justify-content: space-between;
      margin-bottom: 8px; gap: 8px;
    }
    .hdr-name { font-size: 13px; font-weight: 500; overflow: hidden;
                text-overflow: ellipsis; white-space: nowrap; }
    .status-dot {
      width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0;
      background: var(--vscode-testing-iconQueued, #f59e0b);
    }
    .status-dot.ok  { background: var(--vscode-testing-iconPassed, #22c55e); }
    .status-dot.err { background: var(--vscode-testing-iconFailed, #ef4444); }

    /* ── Mode toolbar ── */
    .mode-bar { display: flex; gap: 4px; margin-bottom: 10px; }
    .mode-btn {
      padding: 3px 10px; border-radius: 4px; border: 1px solid transparent;
      cursor: pointer; font-size: 12px; user-select: none;
      background: transparent;
      color: var(--vscode-foreground);
      border-color: var(--vscode-panel-border, #333);
    }
    .mode-btn:hover { opacity: 0.8; }
    .mode-btn.active {
      background: #3b82f6; color: #fff; font-weight: 600; border-color: #3b82f6;
    }

    /* ── Connection hint ── */
    .hint-bar {
      margin-bottom: 10px; padding: 8px 10px; border-radius: 4px;
      background: var(--vscode-inputValidation-warningBackground, #3d2f00);
      border: 1px solid var(--vscode-inputValidation-warningBorder, #855a00);
      font-size: 12px; line-height: 1.5;
    }
    .hint-bar code {
      font-family: var(--vscode-editor-font-family, monospace);
      font-size: 11px;
      background: var(--vscode-textCodeBlock-background, #1e1e1e);
      padding: 1px 4px; border-radius: 2px;
    }
    .hint-copy {
      margin-top: 5px; padding: 2px 8px; font-size: 11px; cursor: pointer;
      background: transparent; color: var(--vscode-foreground);
      border: 1px solid var(--vscode-panel-border, #444);
      border-radius: 3px;
    }

    /* ── Trace list ── */
    .trace-list { display: flex; flex-direction: column; gap: 2px; }
    .trace-row {
      display: flex; align-items: center; gap: 8px; padding: 5px 8px;
      border-radius: 3px; cursor: pointer; border: 1px solid transparent;
    }
    .trace-row:hover { background: var(--vscode-list-hoverBackground); }
    .trace-idx  { font-size: 11px; color: var(--vscode-descriptionForeground);
                  width: 24px; flex-shrink: 0; }
    .trace-icon { font-size: 13px; flex-shrink: 0; }
    .trace-name { font-weight: 500; overflow: hidden; text-overflow: ellipsis;
                  white-space: nowrap; flex: 1; min-width: 0; }
    .trace-meta { font-size: 11px; color: var(--vscode-descriptionForeground);
                  white-space: nowrap; flex-shrink: 0; }
    .empty-msg { color: var(--vscode-descriptionForeground); font-size: 12px;
                 padding: 12px 0; text-align: center; }

    /* ── Stepper ── */
    .stepper { display: flex; flex-direction: column; gap: 0; }
    .stepper-hdr {
      display: flex; align-items: center; justify-content: space-between;
      margin-bottom: 8px; gap: 6px;
    }
    .stepper-title { font-size: 12px; color: var(--vscode-descriptionForeground); }
    .stepper-btns  { display: flex; gap: 4px; }
    .step-btn {
      padding: 2px 10px; border-radius: 3px;
      border: 1px solid var(--vscode-panel-border, #444);
      cursor: pointer; font-size: 12px; user-select: none;
      background: transparent; color: var(--vscode-foreground);
    }
    .step-btn:hover { opacity: 0.8; }
    .step-btn:disabled { opacity: 0.35; cursor: default; }
    .step-btn.accent { background: #3b82f6; color: #fff; border-color: #3b82f6; }
    .back-btn {
      display: inline-block; margin-bottom: 8px; font-size: 11px; cursor: pointer;
      color: var(--vscode-textLink-foreground, #3b82f6); text-decoration: underline;
    }
    .node-list { display: flex; flex-direction: column; gap: 1px; }
    .node-row {
      display: flex; align-items: flex-start; gap: 8px; padding: 5px 8px;
      border-radius: 3px; border: 1px solid transparent;
    }
    .node-row.current { background: #ffe06633; border-color: #f59e0b; }
    .node-idx { font-size: 11px; color: var(--vscode-descriptionForeground);
                width: 18px; flex-shrink: 0; padding-top: 1px; }
    .node-name { font-weight: 500; flex: 1; min-width: 0; overflow: hidden;
                 text-overflow: ellipsis; white-space: nowrap; }
    .node-meta { font-size: 11px; color: var(--vscode-descriptionForeground);
                 flex-shrink: 0; text-align: right; }
    .node-output {
      font-size: 11px; color: var(--vscode-descriptionForeground);
      margin-top: 2px; font-family: var(--vscode-editor-font-family, monospace);
      overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 220px;
    }
    .node-cell { flex: 1; min-width: 0; display: flex; flex-direction: column; }

    /* ── Replay progress ── */
    .replay-bar { height: 3px; border-radius: 2px;
                  background: var(--vscode-panel-border, #333);
                  margin-bottom: 8px; overflow: hidden; }
    .replay-fill { height: 100%; background: #3b82f6; transition: width 0.3s ease; }

    /* ── Diagram area ── */
    .diagram-area { margin-top: 14px; border-top: 1px solid var(--vscode-panel-border, #333);
                    padding-top: 10px; }
    .diagram-label { font-size: 11px; color: var(--vscode-descriptionForeground);
                     margin-bottom: 6px; }
    .mermaid svg { max-width: 100%; height: auto; display: block; }
  </style>
</head>
<body>
  <!-- Header -->
  <div class="hdr">
    <span class="hdr-name" id="hdr-name">&#9654; ${methodName}</span>
    <span class="status-dot" id="status-dot" title="Connecting\u2026"></span>
  </div>

  <!-- Mode toolbar -->
  <div class="mode-bar">
    <button class="mode-btn active" id="btn-history">&#128203; History</button>
    <button class="mode-btn"        id="btn-step"   >&#9197; Step</button>
  </div>

  <!-- Connection hint (hidden when connected) -->
  <div class="hint-bar" id="hint-bar" style="display:none">
    <div id="hint-msg">Start the trace endpoint:</div>
    <code>PipelineTraceHost.Start(buffer) or app.MapResultFlowTraces(buffer)</code>
    <div><button class="hint-copy" id="btn-hint-copy">Copy snippet</button></div>
  </div>

  <!-- Replay progress bar -->
  <div class="replay-bar" id="replay-bar" style="display:none">
    <div class="replay-fill" id="replay-fill" style="width:0%"></div>
  </div>

  <!-- Trace list (History + Single modes) -->
  <div id="trace-panel">
    <div class="trace-list" id="trace-list">
      <div class="empty-msg" id="empty-msg">Waiting for traces&hellip;</div>
    </div>
  </div>

  <!-- Stepper (Step + Replay modes) -->
  <div id="stepper-panel" style="display:none" class="stepper">
    <span class="back-btn" id="btn-back">&#8592; Back to list</span>
    <div class="stepper-hdr">
      <span class="stepper-title" id="stepper-title">Node 0 of 0</span>
      <div class="stepper-btns">
        <button class="step-btn" id="btn-prev" disabled>&#8592; Prev</button>
        <button class="step-btn" id="btn-next">Next &#8594;</button>
        <button class="step-btn accent" id="btn-play">&#9654; Replay</button>
        <button class="step-btn" id="btn-pause" style="display:none">&#9646;&#9646; Pause</button>
      </div>
    </div>
    <div class="node-list" id="node-list"></div>

    <!-- Diagram node highlight area -->
    <div class="diagram-area" id="diagram-area" style="display:none">
      <div class="diagram-label">&#128202; Pipeline diagram &mdash; highlighted node is current step</div>
      <div class="mermaid" id="diagram-el"></div>
    </div>
  </div>

  <script nonce="${nonce}">
    window.__livePanel__ = { diagram: ${diagramJson}, method: ${methodJson} };
  </script>
  <script nonce="${nonce}" src="${scriptUri}"></script>
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
