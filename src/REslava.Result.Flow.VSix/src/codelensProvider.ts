import * as vscode from 'vscode';

export class ResultFlowCodeLensProvider implements vscode.CodeLensProvider {
    provideCodeLenses(document: vscode.TextDocument): vscode.CodeLens[] {
        const lenses: vscode.CodeLens[] = [];
        const lines = document.getText().split('\n');

        for (let i = 0; i < lines.length; i++) {
            if (/^\s*\[ResultFlow[(\]]/.test(lines[i])) {
                const range = new vscode.Range(i, 0, i, lines[i].length);
                lenses.push(new vscode.CodeLens(range, {
                    title: '▶ Open diagram preview',
                    command: 'reslava._previewMethod',
                    arguments: [document.uri, i]
                }));
            }
        }
        return lenses;
    }
}
