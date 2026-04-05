import * as vscode from 'vscode';
import { resolveClassName, hasDebugProxyForClass } from './diagramResolver';

export class ResultFlowCodeLensProvider implements vscode.CodeLensProvider {
    provideCodeLenses(document: vscode.TextDocument): vscode.CodeLens[] {
        const lenses: vscode.CodeLens[] = [];
        const lines = document.getText().split('\n');

        for (let i = 0; i < lines.length; i++) {
            // ▶ Diagram + ▶ Debug above [ResultFlow] attribute declarations
            if (/^\s*\[ResultFlow[(\]]/.test(lines[i])) {
                const range = new vscode.Range(i, 0, i, lines[i].length);
                lenses.push(new vscode.CodeLens(range, {
                    title: '▶ Diagram',
                    command: 'reslava._previewMethod',
                    arguments: [document.uri, i]
                }));
                const className = resolveClassName(document, i);
                if (hasDebugProxyForClass(className)) {
                    lenses.push(new vscode.CodeLens(range, {
                        title: '▶ Debug',
                        command: 'resultflow.openDebugPanel',
                        arguments: [document.uri, i]
                    }));
                }
            }

            // ▶ Debug above .Flow.Debug. call sites (e.g. svc.Flow.Debug.Process(42, 7))
            if (/\.Flow\.Debug\./.test(lines[i])) {
                const methodMatch = lines[i].match(/\.Flow\.Debug\.(\w+)/);
                const methodName = methodMatch ? methodMatch[1] : 'Debug';
                const range = new vscode.Range(i, 0, i, lines[i].length);
                lenses.push(new vscode.CodeLens(range, {
                    title: '▶ Debug',
                    command: 'resultflow.openDebugPanel',
                    arguments: [methodName]
                }));
            }
        }
        return lenses;
    }
}
