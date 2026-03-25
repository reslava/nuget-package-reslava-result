import * as vscode from 'vscode';
import { ResultFlowCodeLensProvider } from './codelensProvider';
import { openPreviewForMethod, refreshDiagramsForDocument } from './diagramResolver';
import { registerGutterDecorator } from './gutterDecorator';
import { ResultFlowTreeProvider, ProjectNode } from './resultFlowTreeProvider';
import { notifyAllPanels } from './webviewPanel';

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
            (uri: vscode.Uri, atLine: number, className?: string, methodName?: string) =>
                openPreviewForMethod(uri, atLine, context.extensionUri, className, methodName)
        )
    );

    // Gutter icon — orange R beside every [ResultFlow] line
    registerGutterDecorator(context);

    // Sidebar — Flow Catalog tree view
    const treeProvider = new ResultFlowTreeProvider();
    const treeView = vscode.window.createTreeView('reslavaResultFlowExplorer', {
        treeDataProvider: treeProvider,
        showCollapseAll: true
    });
    context.subscriptions.push(treeView);

    const updateStats = () => {
        const s = treeProvider.getStats();
        treeView.message = s.pipelines === 0
            ? undefined
            : `${s.projects} projects · ${s.pipelines} pipelines · ${s.nodes} nodes`;
    };

    treeProvider.scanWorkspace().then(updateStats);
    treeProvider.onDidChangeTreeData(() => updateStats());

    context.subscriptions.push(
        vscode.commands.registerCommand('reslava.refreshFlowCatalog', () => {
            treeProvider.scanWorkspace();
        })
    );

    // Go to source line for a method node (right-click → Go to Source)
    context.subscriptions.push(
        vscode.commands.registerCommand('reslava.goToSource', async (node: import('./resultFlowTreeProvider').MethodNode) => {
            const line = node.sourceLine ?? 0;
            const range = new vscode.Range(line, 0, line, 0);
            await vscode.window.showTextDocument(node.uri, { selection: range, preserveFocus: false });
        })
    );

    // Build project (triggered from red project node right-click)
    context.subscriptions.push(
        vscode.commands.registerCommand('reslava.buildProject', async (node: ProjectNode) => {
            const task = new vscode.Task(
                { type: 'shell' },
                vscode.TaskScope.Workspace,
                'Build Project',
                'REslava',
                new vscode.ShellExecution(`dotnet build "${node.projectPath}" --no-incremental`)
            );
            const execution = await vscode.tasks.executeTask(task);
            const disposable = vscode.tasks.onDidEndTaskProcess(e => {
                if (e.execution === execution) {
                    disposable.dispose();
                    treeProvider.scanWorkspace();
                }
            });
        })
    );

    // Toggle single/multiple window mode
    context.subscriptions.push(
        vscode.commands.registerCommand('reslava.toggleDiagramWindowMode', async () => {
            const config = vscode.workspace.getConfiguration('reslava');
            const current = config.get<string>('diagramWindowMode', 'single');
            const next = current === 'single' ? 'multiple' : 'single';
            await config.update('diagramWindowMode', next, vscode.ConfigurationTarget.Global);
            vscode.window.showInformationMessage(`REslava: diagram window mode → ${next}`);
        })
    );

    // Sync mode button label in all open panels when setting changes
    context.subscriptions.push(
        vscode.workspace.onDidChangeConfiguration(e => {
            if (e.affectsConfiguration('reslava.diagramWindowMode')) {
                const mode = vscode.workspace.getConfiguration('reslava')
                    .get<string>('diagramWindowMode', 'single');
                notifyAllPanels({ command: 'windowModeChanged', mode });
            }
        })
    );

    // Auto-refresh open panels when a C# file is saved (500ms debounce absorbs
    // format-on-save double-save events)
    let saveTimer: NodeJS.Timeout | undefined;
    context.subscriptions.push(
        vscode.workspace.onDidSaveTextDocument(doc => {
            if (doc.languageId !== 'csharp') { return; }
            clearTimeout(saveTimer);
            saveTimer = setTimeout(() => refreshDiagramsForDocument(doc), 500);
            treeProvider.refresh(doc.uri);
        })
    );
}

export function deactivate(): void {}
