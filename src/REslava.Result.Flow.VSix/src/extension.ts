import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';
import { ResultFlowCodeLensProvider } from './codelensProvider';
import { openPreviewForMethod, refreshDiagramsForDocument } from './diagramResolver';
import { registerGutterDecorator } from './gutterDecorator';
import { ResultFlowTreeProvider, ProjectNode } from './resultFlowTreeProvider';
import { notifyAllPanels } from './webviewPanel';

const TRACK_A_PACKAGES = ['REslava.Result.Flow'];
const TRACK_B_PACKAGES = ['REslava.ResultFlow'];
const DEMO_URL = 'https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/REslava.Result.Flow.Demo';

async function checkForResultFlowPackages(): Promise<boolean> {
    const csprojUris = await vscode.workspace.findFiles('**/*.csproj', '{**/obj/**,**/bin/**,**/node_modules/**}', 30);
    for (const uri of csprojUris) {
        try {
            const content = fs.readFileSync(uri.fsPath, 'utf8');
            if (TRACK_A_PACKAGES.some(p => content.includes(p)) ||
                TRACK_B_PACKAGES.some(p => content.includes(p))) {
                return true;
            }
        } catch { /* skip unreadable files */ }
    }
    return false;
}

async function findProjectForInstall(): Promise<string | null> {
    const uris = await vscode.workspace.findFiles('**/*.csproj', '{**/obj/**,**/bin/**,**/node_modules/**}', 20);
    if (uris.length === 0) { return null; }
    if (uris.length === 1) { return uris[0].fsPath; }
    const items = uris.map(u => ({
        label: path.basename(u.fsPath, '.csproj'),
        description: vscode.workspace.asRelativePath(u.fsPath),
        fsPath: u.fsPath
    }));
    const picked = await vscode.window.showQuickPick(items, {
        placeHolder: 'Select the project to install the package into'
    });
    return picked ? picked.fsPath : null;
}

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

    // Missing-package detection — notify once per workspace if no ResultFlow package found
    checkForResultFlowPackages().then(found => {
        if (!found) {
            vscode.window.showInformationMessage(
                'REslava.Result Extensions: No ResultFlow package found in your workspace.',
                'Install Track A',
                'Install Track B',
                'See Demo'
            ).then(choice => {
                if (choice === 'Install Track A') {
                    vscode.commands.executeCommand('reslava.installTrackA');
                } else if (choice === 'Install Track B') {
                    vscode.commands.executeCommand('reslava.installTrackB');
                } else if (choice === 'See Demo') {
                    vscode.env.openExternal(vscode.Uri.parse(DEMO_URL));
                }
            });
        }
    });

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
            treeProvider.setAllLoading(true);   // spinner while build runs
            const execution = await vscode.tasks.executeTask(task);
            const disposable = vscode.tasks.onDidEndTaskProcess(e => {
                if (e.execution === execution) {
                    disposable.dispose();
                    treeProvider.scanWorkspace();
                }
            });
        })
    );

    // Install commands (used by notification + walkthrough)
    context.subscriptions.push(
        vscode.commands.registerCommand('reslava.installTrackA', async () => {
            const proj = await findProjectForInstall();
            const p = proj ? `"${proj}" ` : '';
            const terminal = vscode.window.createTerminal('REslava Install');
            terminal.sendText(`dotnet add ${p}package REslava.Result && dotnet add ${p}package REslava.Result.Flow`);
            terminal.show();
        })
    );
    context.subscriptions.push(
        vscode.commands.registerCommand('reslava.installTrackB', async () => {
            const proj = await findProjectForInstall();
            const p = proj ? `"${proj}" ` : '';
            const terminal = vscode.window.createTerminal('REslava Install');
            terminal.sendText(`dotnet add ${p}package REslava.ResultFlow`);
            terminal.show();
        })
    );
    context.subscriptions.push(
        vscode.commands.registerCommand('reslava.openDemo', () => {
            vscode.env.openExternal(vscode.Uri.parse(DEMO_URL));
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
