import * as vscode from 'vscode';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import { ResultFlowCodeLensProvider } from './codelensProvider';
import { openPreviewForMethod, openMarkdownPreview } from './diagramResolver';

const TEMP_DIR = path.join(os.tmpdir(), 'REslava.ResultFlow');

export function activate(context: vscode.ExtensionContext): void {
    // CodeLens — "▶ Open diagram preview" above every [ResultFlow] method
    context.subscriptions.push(
        vscode.languages.registerCodeLensProvider(
            { language: 'csharp' },
            new ResultFlowCodeLensProvider()
        )
    );

    // Internal command fired by CodeLens click — runs the 4-step fallback chain
    context.subscriptions.push(
        vscode.commands.registerCommand(
            'reslava._previewMethod',
            (uri: vscode.Uri, atLine: number) => openPreviewForMethod(uri, atLine)
        )
    );

    // Public command — fired by the Option A file watcher (and usable from command palette)
    context.subscriptions.push(
        vscode.commands.registerCommand(
            'reslava.openDiagramPreview',
            (filePath: string) => openMarkdownPreview(filePath)
        )
    );

    // Option A bridge: watch temp folder for .md files written by external tools
    startTempFolderWatcher(context);
}

function startTempFolderWatcher(context: vscode.ExtensionContext): void {
    fs.mkdirSync(TEMP_DIR, { recursive: true });

    const watcher = fs.watch(TEMP_DIR, (event: string, filename: string | null) => {
        if (!filename || !filename.endsWith('.md')) { return; }
        if (event !== 'rename' && event !== 'change') { return; }

        const filePath = path.join(TEMP_DIR, filename);
        setTimeout(() => {
            if (fs.existsSync(filePath)) {
                openMarkdownPreview(filePath);
            }
        }, 150);
    });

    context.subscriptions.push({ dispose: () => watcher.close() });
}

export function deactivate(): void {}
