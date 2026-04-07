import * as vscode from 'vscode';

/** Opens a read-only webview panel showing AI-generated output with a Copy button. */
export function showAIResult(title: string, content: string): void {
    const panel = vscode.window.createWebviewPanel(
        'resultflow.aiResult',
        title,
        vscode.ViewColumn.Beside,
        { enableScripts: true }
    );
    panel.webview.html = buildAIPanelHtml(content);
}

function buildAIPanelHtml(content: string): string {
    const escaped = content
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');

    const nonce = getNonce();

    return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <meta http-equiv="Content-Security-Policy"
        content="default-src 'none'; script-src 'nonce-${nonce}'; style-src 'unsafe-inline';" />
  <title>AI Result</title>
  <style>
    *, *::before, *::after { box-sizing: border-box; }
    body {
      margin: 0; padding: 16px;
      background: var(--vscode-editor-background);
      color: var(--vscode-editor-foreground);
      font-family: var(--vscode-font-family, sans-serif);
      font-size: 13px;
    }
    .toolbar { margin-bottom: 12px; display: flex; align-items: center; gap: 10px; }
    .copy-btn {
      padding: 4px 12px; border-radius: 3px; cursor: pointer; font-size: 12px;
      background: #3b82f6; color: #fff; border: none;
    }
    .copy-btn:hover { opacity: 0.85; }
    .copy-ok { font-size: 12px; color: #22c55e; }
    pre {
      font-family: var(--vscode-editor-font-family, monospace);
      font-size: 12px; line-height: 1.5;
      background: var(--vscode-textCodeBlock-background, #1e1e1e);
      padding: 14px; border-radius: 4px;
      overflow-x: auto; white-space: pre-wrap; word-break: break-word;
      margin: 0;
    }
  </style>
</head>
<body>
  <div class="toolbar">
    <button class="copy-btn" id="copy-btn">&#128203; Copy</button>
    <span class="copy-ok" id="copy-ok" style="display:none">Copied!</span>
  </div>
  <pre id="content">${escaped}</pre>
  <script nonce="${nonce}">
    const content = document.getElementById('content').textContent;
    document.getElementById('copy-btn').addEventListener('click', () => {
      navigator.clipboard.writeText(content).then(() => {
        const ok = document.getElementById('copy-ok');
        ok.style.display = '';
        setTimeout(() => { ok.style.display = 'none'; }, 2000);
      });
    });
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
