import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';
import { showWebviewPanel, refreshWebviewPanel, isPanelOpen } from './webviewPanel';

interface DiagramResult {
    diagram:          string;
    typeFlow:         string | null;
    layerView:        string | null;
    stats:            string | null;
    errorSurface:     string | null;
    errorPropagation: string | null;
}

// ─── Auto-refresh on save ────────────────────────────────────────────────────
//
// Called by the onDidSaveTextDocument listener (extension.ts).
// For each [ResultFlow] method in the saved document, if a panel is already
// open, re-reads the generated *_Flows.g.cs and posts an update to the panel.
// No-op for methods whose panel has not been opened yet.

export function refreshDiagramsForDocument(document: vscode.TextDocument): void {
    const lines = document.getText().split('\n');
    for (let i = 0; i < lines.length; i++) {
        if (!/^\s*\[ResultFlow[(\]]/.test(lines[i])) { continue; }

        const className  = resolveClassName(document, i);
        const methodName = resolveMethodName(document, i);

        // Only hit the disk if the panel is already open
        if (!isPanelOpen(methodName)) { continue; }

        const result = findDiagramInGeneratedFile(className, methodName);
        if (result) {
            refreshWebviewPanel(
                methodName, result.diagram, result.typeFlow,
                result.layerView, result.stats, result.errorSurface, result.errorPropagation
            );
        }
    }
}

// ─── Entry point (called by command on CodeLens click or sidebar tree click) ──
//
// knownClassName / knownMethodName: when passed from the sidebar tree provider
// (which already has these from the registry), text-parsing is skipped entirely
// and step 1 goes straight to findDiagramInGeneratedFile.

export async function openPreviewForMethod(
    uri: vscode.Uri,
    atLine: number,
    extensionUri: vscode.Uri,
    knownClassName?: string,
    knownMethodName?: string
): Promise<void> {
    try {
        const document   = await vscode.workspace.openTextDocument(uri);
        const methodName = knownMethodName ?? resolveMethodName(document, atLine);
        const className  = knownClassName  ?? resolveClassName(document, atLine);

        // Step 1 — generated *_Flows.g.cs (primary source)
        const fromGenerated = findDiagramInGeneratedFile(className, methodName);
        if (fromGenerated) {
            const hasTrace = hasTraceForMethod(className);
            showWebviewPanel(
                methodName, fromGenerated.diagram, extensionUri,
                fromGenerated.typeFlow, fromGenerated.layerView,
                fromGenerated.stats, fromGenerated.errorSurface, fromGenerated.errorPropagation,
                hasTrace
            );
            return;
        }

        // Step 2 — auto-run "Insert diagram as comment" code action (Roslyn)
        const fromCodeAction = await tryInsertViaCodeAction(document, atLine);
        if (fromCodeAction) {
            showWebviewPanel(methodName, fromCodeAction, extensionUri);
            return;
        }

        // Step 3 — existing /*```mermaid...```*/ comment already in source
        const updatedDoc = await vscode.workspace.openTextDocument(uri); // re-read after possible edit
        const fromComment = extractFromExistingComment(updatedDoc, atLine);
        if (fromComment) {
            showWebviewPanel(methodName, fromComment, extensionUri);
            return;
        }

        // Step 4 — not ready yet
        vscode.window.showInformationMessage(
            'REslava: diagram not ready yet — build the project or try again in a moment.'
        );
    } catch {
        vscode.window.showInformationMessage(
            'REslava: diagram not ready yet — VS Code is still loading, please try again in a moment.'
        );
    }
}

// ─── Step 1: *_Flows.g.cs ────────────────────────────────────────────────────
//
// vscode.workspace.findFiles() is unreliable for files inside obj/ even with
// null exclude: the VS Code file watcher may not index gitignored dirs.
// We walk the workspace folder(s) directly using fs.readdirSync — a plain OS
// call that bypasses all VS Code indexing and .gitignore logic entirely.

function findDiagramInGeneratedFile(className: string, methodName: string): DiagramResult | null {
    const targetFile = `${className}_Flows.g.cs`;
    const folders = vscode.workspace.workspaceFolders ?? [];
    for (const folder of folders) {
        const filePath = walkForObjFile(folder.uri.fsPath, targetFile);
        if (filePath) {
            const content = fs.readFileSync(filePath, 'utf8');
            const diagram = extractDiagramConstant(content, methodName);
            if (!diagram) { return null; }
            return {
                diagram,
                typeFlow:         extractDiagramConstant(content, `${methodName}_TypeFlow`),
                layerView:        extractDiagramConstant(content, `${methodName}_LayerView`),
                stats:            extractDiagramConstant(content, `${methodName}_Stats`),
                errorSurface:     extractDiagramConstant(content, `${methodName}_ErrorSurface`),
                errorPropagation: extractDiagramConstant(content, `${methodName}_ErrorPropagation`),
            };
        }
    }
    return null;
}

// Directories to skip while walking the workspace (large, never contain generated files).
const SKIP_DIRS = new Set([
    'node_modules', '.git', 'bin', '.vs', '.idea', 'packages', '.cache', '.next', 'dist'
]);

// Two-phase walk: traverse workspace looking for 'obj' dirs, then search inside each.
function walkForObjFile(dir: string, targetFile: string): string | null {
    let entries: fs.Dirent[];
    try { entries = fs.readdirSync(dir, { withFileTypes: true }); }
    catch { return null; }

    for (const entry of entries) {
        if (!entry.isDirectory()) { continue; }
        const fullPath = path.join(dir, entry.name);
        if (entry.name === 'obj') {
            const found = findInDirRecursive(fullPath, targetFile);
            if (found) { return found; }
        } else if (!SKIP_DIRS.has(entry.name)) {
            const found = walkForObjFile(fullPath, targetFile);
            if (found) { return found; }
        }
    }
    return null;
}

// Simple recursive file search inside a known directory.
function findInDirRecursive(dir: string, targetFile: string): string | null {
    let entries: fs.Dirent[];
    try { entries = fs.readdirSync(dir, { withFileTypes: true }); }
    catch { return null; }

    for (const entry of entries) {
        const fullPath = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            const found = findInDirRecursive(fullPath, targetFile);
            if (found) { return found; }
        } else if (entry.name === targetFile) {
            return fullPath;
        }
    }
    return null;
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

    // The edit inserts N lines above [ResultFlow], shifting it down to atLine+N.
    // Re-read the document and find the NEW position of [ResultFlow] before searching
    // for the comment — the proximity check uses nearLine, which must be the updated line.
    const updated = await vscode.workspace.openTextDocument(document.uri);
    let newAtLine = atLine;
    for (let i = atLine; i < Math.min(atLine + 150, updated.lineCount); i++) {
        if (/^\s*\[ResultFlow\b/.test(updated.lineAt(i).text)) {
            newAtLine = i;
            break;
        }
    }
    return extractFromExistingComment(updated, newAtLine);
}

// ─── Step 3: existing /*```mermaid...```*/ in source ────────────────────────

function extractFromExistingComment(
    document: vscode.TextDocument,
    nearLine: number
): string | null {
    const text = document.getText();
    // \s* after /* swallows the newline (handles both LF and CRLF)
    const commentRegex = /\/\*\s*```mermaid[\r\n]+([\s\S]*?)```\s*\*\//g;
    let match: RegExpExecArray | null;

    while ((match = commentRegex.exec(text)) !== null) {
        // Check proximity by END of comment — closing ```*/ is always just
        // 1-2 lines above [ResultFlow], regardless of how long the diagram is
        const endLine = document.positionAt(match.index + match[0].length).line;
        if (endLine <= nearLine && nearLine - endLine <= 10) {
            return match[1].trim();
        }
    }
    return null;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export function resolveMethodName(document: vscode.TextDocument, fromLine: number): string {
    for (let i = fromLine; i < Math.min(fromLine + 5, document.lineCount); i++) {
        const m = document.lineAt(i).text.match(
            /(?:public|private|protected|internal|async|static|\s)+\S+\s+(\w+)\s*[(<]/
        );
        if (m) { return m[1]; }
    }
    return 'Pipeline';
}

export function resolveClassName(document: vscode.TextDocument, fromLine: number): string {
    for (let i = fromLine; i >= 0; i--) {
        // After the class name, C# requires : { < ( or end-of-line.
        // Mermaid "class Foo subgraphStyle" has a plain word after the name — skip it.
        const m = document.lineAt(i).text.match(
            /(?:^|[\w\s]*?)class\s+(\w+)(?:\s*[:({<]|\s*$)/
        );
        if (m) { return m[1]; }
    }
    return 'Unknown';
}

// ─── Trace detection ─────────────────────────────────────────────────────────
//
// Returns true when the generated *_Flows.g.cs for className contains a
// {ClassName}_Traced_Extensions class — meaning the class is an instance-method
// [ResultFlow] class for which the generator emitted _Traced wrappers.
// Static-method classes never get _Traced, so this returns false for them.

export function hasTraceForMethod(className: string): boolean {
    const targetFile = `${className}_Flows.g.cs`;
    const folders = vscode.workspace.workspaceFolders ?? [];
    for (const folder of folders) {
        const filePath = walkForObjFile(folder.uri.fsPath, targetFile);
        if (filePath) {
            try {
                const content = fs.readFileSync(filePath, 'utf8');
                return content.includes(`${className}_Traced_Extensions`);
            } catch { return false; }
        }
    }
    return false;
}

// ─── Live panel helper ────────────────────────────────────────────────────────
//
// Searches all *_Flows.g.cs files in the workspace for a diagram constant
// matching methodName. Used by the Live panel when only methodName is known.

export function findDiagramByMethodName(methodName: string): string | null {
    const folders = vscode.workspace.workspaceFolders ?? [];
    for (const folder of folders) {
        const result = searchWorkspaceForMethod(folder.uri.fsPath, methodName);
        if (result) { return result; }
    }
    return null;
}

function searchWorkspaceForMethod(dir: string, methodName: string): string | null {
    let entries: fs.Dirent[];
    try { entries = fs.readdirSync(dir, { withFileTypes: true }); }
    catch { return null; }

    for (const entry of entries) {
        if (!entry.isDirectory()) { continue; }
        const fullPath = path.join(dir, entry.name);
        if (entry.name === 'obj') {
            const result = searchObjForMethod(fullPath, methodName);
            if (result) { return result; }
        } else if (!SKIP_DIRS.has(entry.name)) {
            const result = searchWorkspaceForMethod(fullPath, methodName);
            if (result) { return result; }
        }
    }
    return null;
}

function searchObjForMethod(dir: string, methodName: string): string | null {
    let entries: fs.Dirent[];
    try { entries = fs.readdirSync(dir, { withFileTypes: true }); }
    catch { return null; }

    for (const entry of entries) {
        const fullPath = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            const result = searchObjForMethod(fullPath, methodName);
            if (result) { return result; }
        } else if (entry.name.endsWith('_Flows.g.cs')) {
            try {
                const content = fs.readFileSync(fullPath, 'utf8');
                const diagram = extractDiagramConstant(content, methodName);
                if (diagram) { return diagram; }
            } catch { /* skip */ }
        }
    }
    return null;
}

// Extracts the diagram string from: public const string MethodName = @"...";
// or the triple-quoted raw string form used in .g.cs files.
function extractDiagramConstant(fileContent: string, methodName: string): string | null {
    const regex = new RegExp(
        `public\\s+const\\s+string\\s+${methodName}\\s*=\\s*@?"([^"]*(?:""[^"]*)*)"`,
        's'
    );
    const m = fileContent.match(regex);
    if (!m) { return null; }
    return m[1].replace(/""/g, '"');
}
