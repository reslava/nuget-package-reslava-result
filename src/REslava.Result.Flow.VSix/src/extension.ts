import * as vscode from 'vscode';
import { ResultFlowCodeLensProvider } from './codelensProvider';
import { openPreviewForMethod } from './diagramResolver';
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
}

export function deactivate(): void {}
