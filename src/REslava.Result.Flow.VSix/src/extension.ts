import * as vscode from 'vscode';
import { ResultFlowCodeLensProvider } from './codelensProvider';
import { openPreviewForMethod, refreshDiagramsForDocument } from './diagramResolver';
import { registerGutterDecorator } from './gutterDecorator';

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
            (uri: vscode.Uri, atLine: number) =>
                openPreviewForMethod(uri, atLine, context.extensionUri)
        )
    );

    // Gutter icon — orange R beside every [ResultFlow] line
    registerGutterDecorator(context);

    // Auto-refresh open panels when a C# file is saved (500ms debounce absorbs
    // format-on-save double-save events)
    let saveTimer: NodeJS.Timeout | undefined;
    context.subscriptions.push(
        vscode.workspace.onDidSaveTextDocument(doc => {
            if (doc.languageId !== 'csharp') { return; }
            clearTimeout(saveTimer);
            saveTimer = setTimeout(() => refreshDiagramsForDocument(doc), 500);
        })
    );
}

export function deactivate(): void {}
