import * as vscode from 'vscode';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';

// ─── Entry point (called by command on CodeLens click) ───────────────────────

export async function openPreviewForMethod(uri: vscode.Uri, atLine: number): Promise<void> {
    const document = await vscode.workspace.openTextDocument(uri);
    const methodName = resolveMethodName(document, atLine);
    const className  = resolveClassName(document, atLine);
    const tempPath   = buildTempPath(className, methodName);

    // Step 1 — generated *_Flows.g.cs
    const fromGenerated = await findDiagramInGeneratedFile(className, methodName);
    if (fromGenerated) {
        writeSidecar(tempPath, methodName, fromGenerated);
        openMarkdownPreview(tempPath);
        return;
    }

    // Step 2 — auto-run "Insert diagram as comment" code action (Roslyn)
    const fromCodeAction = await tryInsertViaCodeAction(document, atLine);
    if (fromCodeAction) {
        writeSidecar(tempPath, methodName, fromCodeAction);
        openMarkdownPreview(tempPath);
        return;
    }

    // Step 3 — existing /*```mermaid...```*/ comment already in source
    const updatedDoc = await vscode.workspace.openTextDocument(uri); // re-read after possible edit
    const fromComment = extractFromExistingComment(updatedDoc, atLine);
    if (fromComment) {
        writeSidecar(tempPath, methodName, fromComment);
        openMarkdownPreview(tempPath);
        return;
    }

    // Step 4 — not ready yet
    vscode.window.showInformationMessage(
        'REslava: diagram not ready yet — build the project or try again in a moment.'
    );
}

export function openMarkdownPreview(filePath: string): void {
    if (!fs.existsSync(filePath)) {
        vscode.window.showErrorMessage(`REslava: diagram file not found: ${filePath}`);
        return;
    }
    vscode.commands.executeCommand('markdown.showPreviewToSide', vscode.Uri.file(filePath));
}

// ─── Step 1: *_Flows.g.cs ────────────────────────────────────────────────────

async function findDiagramInGeneratedFile(
    className: string,
    methodName: string
): Promise<string | null> {
    const files = await vscode.workspace.findFiles(
        `**/${className}_Flows.g.cs`,
        '**/node_modules/**',
        1
    );
    if (files.length === 0) { return null; }

    const content = fs.readFileSync(files[0].fsPath, 'utf8');
    return extractDiagramConstant(content, methodName);
}

// ─── Step 2: auto-run "Insert diagram as comment" code action ────────────────

async function tryInsertViaCodeAction(
    document: vscode.TextDocument,
    atLine: number
): Promise<string | null> {
    const range = new vscode.Range(atLine, 0, atLine, document.lineAt(atLine).text.length);

    let actions: vscode.CodeAction[] | undefined;
    try {
        actions = await vscode.commands.executeCommand<vscode.CodeAction[]>(
            'vscode.executeCodeActionProvider',
            document.uri,
            range
        );
    } catch {
        return null; // Roslyn not ready
    }

    if (!actions || actions.length === 0) { return null; }

    const insertAction = actions.find(a =>
        a.title.toLowerCase().includes('insert') &&
        a.title.toLowerCase().includes('diagram')
    );
    if (!insertAction?.edit) { return null; }

    const applied = await vscode.workspace.applyEdit(insertAction.edit);
    if (!applied) { return null; }

    // Re-read document and scan for the freshly-inserted comment
    // The comment is inserted above [ResultFlow], so search within ±30 lines
    const updated = await vscode.workspace.openTextDocument(document.uri);
    return extractFromExistingComment(updated, atLine);
}

// ─── Step 3: existing /*```mermaid...```*/ in source ────────────────────────

function extractFromExistingComment(
    document: vscode.TextDocument,
    nearLine: number
): string | null {
    const text = document.getText();
    // Match block comments containing a mermaid fence
    const commentRegex = /\/\*\s*\n```mermaid\n([\s\S]*?)```\s*\*\//g;
    let match: RegExpExecArray | null;

    while ((match = commentRegex.exec(text)) !== null) {
        const matchLine = document.positionAt(match.index).line;
        if (Math.abs(matchLine - nearLine) < 40) {
            return match[1].trim();
        }
    }
    return null;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

function resolveMethodName(document: vscode.TextDocument, fromLine: number): string {
    for (let i = fromLine; i < Math.min(fromLine + 5, document.lineCount); i++) {
        const m = document.lineAt(i).text.match(
            /(?:public|private|protected|internal|async|static|\s)+\S+\s+(\w+)\s*[(<]/
        );
        if (m) { return m[1]; }
    }
    return 'Pipeline';
}

function resolveClassName(document: vscode.TextDocument, fromLine: number): string {
    for (let i = fromLine; i >= 0; i--) {
        const m = document.lineAt(i).text.match(/class\s+(\w+)/);
        if (m) { return m[1]; }
    }
    return 'Unknown';
}

function buildTempPath(className: string, methodName: string): string {
    const dir = path.join(os.tmpdir(), 'REslava.ResultFlow');
    fs.mkdirSync(dir, { recursive: true });
    return path.join(dir, `${className}_${methodName}.md`);
}

function writeSidecar(tempPath: string, methodName: string, diagram: string): void {
    const content = `# Pipeline \u2014 ${methodName}\n\n\`\`\`mermaid\n${diagram.trim()}\n\`\`\`\n`;
    fs.writeFileSync(tempPath, content, 'utf8');
}

// Extracts the diagram string from: public const string MethodName = @"...";
function extractDiagramConstant(fileContent: string, methodName: string): string | null {
    const regex = new RegExp(
        `public\\s+const\\s+string\\s+${methodName}\\s*=\\s*@?"([^"]*(?:""[^"]*)*)"`,
        's'
    );
    const m = fileContent.match(regex);
    if (!m) { return null; }
    return m[1].replace(/""/g, '"');
}
