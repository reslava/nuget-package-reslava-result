import * as vscode from 'vscode';
import { resolveClassName, hasTraceForMethod } from './diagramResolver';

export class ResultFlowCodeLensProvider implements vscode.CodeLensProvider {
    provideCodeLenses(document: vscode.TextDocument): vscode.CodeLens[] {
        const lenses: vscode.CodeLens[] = [];
        const lines = document.getText().split('\n');

        for (let i = 0; i < lines.length; i++) {
            if (/^\s*\[ResultFlow[(\]]/.test(lines[i])) {
                const range = new vscode.Range(i, 0, i, lines[i].length);
                lenses.push(new vscode.CodeLens(range, {
                    title: '▶ Diagram',
                    command: 'reslava._previewMethod',
                    arguments: [document.uri, i]
                }));
                const className = resolveClassName(document, i);
                if (hasTraceForMethod(className)) {
                    lenses.push(new vscode.CodeLens(range, {
                        title: '▶ Debug',
                        command: 'resultflow.openLivePanel',
                        arguments: [document.uri, i]
                    }));
                }
            }
        }
        return lenses;
    }
}
