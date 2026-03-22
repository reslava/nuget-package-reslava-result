import * as vscode from 'vscode';

// Must start with `[` after trimming whitespace — excludes comment lines like // [ResultFlow...]
const RESULT_FLOW_REGEX = /^\s*\[ResultFlow\b/;

let decorationType: vscode.TextEditorDecorationType | undefined;

export function registerGutterDecorator(context: vscode.ExtensionContext): void {
    decorationType = vscode.window.createTextEditorDecorationType({
        gutterIconPath: context.asAbsolutePath('images/icon.png'),
        gutterIconSize: 'contain'
    });

    // Apply to any already-open C# editors
    vscode.window.visibleTextEditors
        .filter(e => e.document.languageId === 'csharp')
        .forEach(applyDecorations);

    context.subscriptions.push(
        vscode.window.onDidChangeActiveTextEditor(editor => {
            if (editor && editor.document.languageId === 'csharp') {
                applyDecorations(editor);
            }
        }),
        vscode.workspace.onDidChangeTextDocument(event => {
            const editor = vscode.window.activeTextEditor;
            if (editor && editor.document === event.document && editor.document.languageId === 'csharp') {
                applyDecorations(editor);
            }
        }),
        { dispose: () => decorationType?.dispose() }
    );
}

function applyDecorations(editor: vscode.TextEditor): void {
    if (!decorationType) { return; }

    const ranges: vscode.Range[] = [];
    const doc = editor.document;

    for (let i = 0; i < doc.lineCount; i++) {
        if (RESULT_FLOW_REGEX.test(doc.lineAt(i).text)) {
            ranges.push(new vscode.Range(i, 0, i, 0));
        }
    }

    editor.setDecorations(decorationType, ranges);
}
