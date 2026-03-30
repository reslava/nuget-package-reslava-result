import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';
import { hasTraceForMethod } from './diagramResolver';

const RESULT_FLOW_ATTR = /^\s*\[ResultFlow[(\]]/;
const CLASS_DECL       = /(?:^|[\w\s]*?)class\s+(\w+)(?:\s*[:({<]|\s*$)/;
const METHOD_DECL      = /(?:public|private|protected|internal|async|static|\s)+\S+\s+(\w+)\s*[(<]/;

export type FlowTreeNode = ProjectNode | NamespaceNode | ClassNode | MethodNode | ErrorNode | LoadingPlaceholderItem | EmptyStateItem;

interface RegistryMethodInfo {
    sourceLine:         number;
    returnType:         string;
    returnTypeFullName: string;
    isAsync:            boolean;
    hasDiagram:         boolean;
    nodeCount?:         number;
    errorTypes?:        string[];
    nodeKindFlags?:     string[];
    namespace?:         string;
    pipelineId?:        string;
}

interface ProjectEntry {
    node:         ProjectNode;
    fileClasses:  Map<string, ClassNode[]>;
    hasRegistry:  boolean;
}

export class ResultFlowTreeProvider
    implements vscode.TreeDataProvider<FlowTreeNode>
{
    private _onDidChangeTreeData =
        new vscode.EventEmitter<FlowTreeNode | undefined | void>();
    readonly onDidChangeTreeData = this._onDidChangeTreeData.event;

    private cache         = new Map<string, ProjectEntry>();
    private fileToProject = new Map<string, string>();
    private _isBuilding   = false;
    private _hasScanned   = false;

    getTreeItem(node: FlowTreeNode): vscode.TreeItem { return node; }

    getChildren(node?: FlowTreeNode): FlowTreeNode[] {
        if (!node) {
            const projects = [...this.cache.values()]
                .map(e => e.node)
                .filter(p => p.classes.length > 0)
                .sort((a, b) => a.projectName.localeCompare(b.projectName));
            if (this._isBuilding && projects.length === 0) {
                return [new LoadingPlaceholderItem()];
            }
            if (this._hasScanned && projects.length === 0) {
                return [new EmptyStateItem()];
            }
            return projects;
        }
        if (node instanceof ProjectNode) {
            // Show namespace level when at least one class has a real namespace
            const hasNamespaces = node.namespaces.some(n => n.namespaceName !== '');
            return hasNamespaces ? node.namespaces : node.classes;
        }
        if (node instanceof NamespaceNode) { return node.classes; }
        if (node instanceof ClassNode)     { return node.methods; }
        if (node instanceof MethodNode)    { return node.errorChildren; }
        return [];
    }

    /** Shows / hides the spinner on all cached project nodes. Call before long operations. */
    setAllLoading(loading: boolean): void {
        for (const entry of this.cache.values()) {
            entry.node.setLoading(loading);
        }
        this._onDidChangeTreeData.fire(undefined);
    }

    async scanWorkspace(): Promise<void> {
        this._isBuilding = true;
        this.setAllLoading(true);   // spinner on existing nodes while we rebuild
        this.cache.clear();
        this.fileToProject.clear();
        this._onDidChangeTreeData.fire(undefined);  // show LoadingPlaceholderItem while cache is empty

        const csprojUris = await vscode.workspace.findFiles('**/*.csproj', '{**/obj/**,**/bin/**,**/node_modules/**}');
        const projects   = csprojUris.map(u => ({ fsPath: u.fsPath, dir: path.dirname(u.fsPath) }));

        const csFiles = await vscode.workspace.findFiles(
            '**/*.cs',
            '{**/obj/**,**/node_modules/**,**/bin/**}'
        );

        for (const csUri of csFiles) {
            const projectPath = findOwnerProject(csUri.fsPath, projects);
            if (!projectPath) { continue; }

            if (!this.cache.has(projectPath)) {
                const name   = path.basename(projectPath, '.csproj');
                const relDir = vscode.workspace.asRelativePath(path.dirname(projectPath));
                this.cache.set(projectPath, {
                    node:        new ProjectNode(name, projectPath, relDir),
                    fileClasses: new Map(),
                    hasRegistry: false
                });
            }

            this.fileToProject.set(csUri.fsPath, projectPath);
            this.scanFileIntoProject(csUri.fsPath, projectPath);
        }

        // Phase B + C: enrich all projects from registry files
        for (const [projectPath, entry] of this.cache) {
            this.enrichFromRegistry(projectPath, entry);
        }

        this._isBuilding = false;
        this._hasScanned = true;
        this._onDidChangeTreeData.fire(undefined);
    }

    refresh(uri?: vscode.Uri): void {
        this._isBuilding = true;
        this.setAllLoading(true);   // spinner while re-scanning; enrichFromRegistry restores icon
        if (uri) {
            const projectPath = this.fileToProject.get(uri.fsPath);
            if (projectPath) {
                this.scanFileIntoProject(uri.fsPath, projectPath);
                const entry = this.cache.get(projectPath);
                if (entry) { this.enrichFromRegistry(projectPath, entry); }
            }
        }
        this._isBuilding = false;
        this._onDidChangeTreeData.fire(undefined);
    }

    // Phase D: stats for treeView.message — only count projects visible in the tree
    getStats(): { projects: number; pipelines: number; nodes: number } {
        let projects  = 0;
        let pipelines = 0;
        let nodes     = 0;
        for (const entry of this.cache.values()) {
            if (entry.node.classes.length === 0) { continue; }
            projects++;
            for (const cls of entry.node.classes) {
                pipelines += cls.methods.length;
                for (const m of cls.methods) { nodes += m.nodeCount ?? 0; }
            }
        }
        return { projects, pipelines, nodes };
    }

    private scanFileIntoProject(filePath: string, projectPath: string): void {
        const entry = this.cache.get(projectPath);
        if (!entry) { return; }

        try {
            const text    = fs.readFileSync(filePath, 'utf8');
            const lines   = text.split(/\r?\n/);
            const classes: ClassNode[] = [];
            let currentClass: ClassNode | null = null;
            const relPath = vscode.workspace.asRelativePath(filePath);

            for (let i = 0; i < lines.length; i++) {
                const classMatch = lines[i].match(CLASS_DECL);
                if (classMatch) {
                    currentClass = new ClassNode(classMatch[1], filePath, relPath);
                    classes.push(currentClass);
                    continue;
                }
                if (RESULT_FLOW_ATTR.test(lines[i]) && currentClass) {
                    const resolved = resolveMethodInfo(lines, i + 1);
                    if (resolved && !currentClass.methods.some(m => m.methodName === resolved.name)) {
                        currentClass.methods.push(
                            new MethodNode(resolved.name, vscode.Uri.file(filePath), i, resolved.isAsync, currentClass.className)
                        );
                    }
                }
            }

            // Deduplicate class nodes by name (handles string literals in test files)
            const deduped = new Map<string, ClassNode>();
            for (const cls of classes) {
                if (!deduped.has(cls.className)) {
                    deduped.set(cls.className, cls);
                } else {
                    const existing = deduped.get(cls.className)!;
                    for (const m of cls.methods) {
                        if (!existing.methods.some(x => x.methodName === m.methodName)) {
                            existing.methods.push(m);
                        }
                    }
                }
            }

            entry.fileClasses.set(filePath, [...deduped.values()].filter(c => c.methods.length > 0));
        } catch {
            entry.fileClasses.delete(filePath);
        }

        entry.node.classes = [...entry.fileClasses.values()].flat()
            .sort((a, b) => a.className.localeCompare(b.className));
    }

    private enrichFromRegistry(projectPath: string, entry: ProjectEntry): void {
        const projectDir   = path.dirname(projectPath);
        const registryData = findRegistryFiles(projectDir);  // className → Map<methodName, info>

        entry.hasRegistry = registryData.size > 0;
        entry.node.setHasRegistry(entry.hasRegistry);

        for (const cls of entry.node.classes) {
            const methodInfoMap = registryData.get(cls.className);
            if (!methodInfoMap) { continue; }
            const classHasTrace = hasTraceForMethod(cls.className);
            for (const method of cls.methods) {
                const info = methodInfoMap.get(method.methodName);
                if (info) {
                    method.applyRegistryInfo(info, cls.className, classHasTrace);
                    // Seed namespace from first method that has one
                    if (!cls.namespace && info.namespace) { cls.namespace = info.namespace; }
                }
            }
        }

        entry.node.buildNamespaces();
    }
}

// ─── Tree items ───────────────────────────────────────────────────────────────

export class ProjectNode extends vscode.TreeItem {
    classes: ClassNode[]       = [];
    namespaces: NamespaceNode[] = [];
    private _hasRegistry = false;

    constructor(
        readonly projectName: string,
        readonly projectPath: string,
        relDir: string
    ) {
        super(projectName, vscode.TreeItemCollapsibleState.Expanded);
        this.description  = relDir;
        this.contextValue = 'reslavaProject';
        this.iconPath     = new vscode.ThemeIcon('loading~spin');
    }

    buildNamespaces(): void {
        const map = new Map<string, ClassNode[]>();
        for (const cls of this.classes) {
            const key = truncateNamespace(cls.namespace);
            if (!map.has(key)) { map.set(key, []); }
            map.get(key)!.push(cls);
        }
        this.namespaces = [...map.entries()]
            .sort((a, b) => a[0].localeCompare(b[0]))
            .map(([ns, classes]) => {
                const node = new NamespaceNode(ns);
                node.classes.push(...classes);
                return node;
            });
    }

    setLoading(loading: boolean): void {
        if (loading) {
            this.iconPath = new vscode.ThemeIcon('loading~spin');
        } else {
            this.setHasRegistry(this._hasRegistry);
        }
    }

    setHasRegistry(has: boolean): void {
        this._hasRegistry = has;
        if (has) {
            this.iconPath = new vscode.ThemeIcon('package',
                new vscode.ThemeColor('testing.iconPassed'));
            this.tooltip  = undefined;
        } else {
            this.iconPath = new vscode.ThemeIcon('package',
                new vscode.ThemeColor('testing.iconFailed'));
            this.tooltip  = new vscode.MarkdownString(
                '**Needs build** — no registry found.  \nRight-click → Build Project');
        }
    }
}

export class NamespaceNode extends vscode.TreeItem {
    readonly classes: ClassNode[] = [];
    constructor(readonly namespaceName: string) {
        super(namespaceName || '(root)', vscode.TreeItemCollapsibleState.Expanded);
        this.iconPath     = new vscode.ThemeIcon('symbol-namespace');
        this.contextValue = 'reslavaNamespace';
    }
}

export class ClassNode extends vscode.TreeItem {
    readonly methods: MethodNode[] = [];
    namespace: string = '';
    constructor(
        readonly className: string,
        readonly filePath: string,
        relativePath: string
    ) {
        super(className, vscode.TreeItemCollapsibleState.Collapsed);
        this.description = relativePath;
        this.iconPath    = new vscode.ThemeIcon('symbol-class');
    }
}

export class MethodNode extends vscode.TreeItem {
    nodeCount: number | null  = null;
    sourceLine: number | null = null;  // 0-based, for goToSource command
    pipelineId: string | null = null;  // FNV-1a hash, for Live panel correlation
    errorChildren: ErrorNode[] = [];

    constructor(
        readonly methodName: string,
        readonly uri: vscode.Uri,
        lineNumber: number,
        isAsync: boolean = false,
        readonly className: string = ''
    ) {
        super(methodName + (isAsync ? '⚡' : ''), vscode.TreeItemCollapsibleState.None);
        this.iconPath    = new vscode.ThemeIcon('symbol-method');
        this.contextValue = 'reslavaMethod';
        this.command  = {
            command:   'reslava._previewMethod',
            title:     'Open Diagram Preview',
            arguments: [uri, lineNumber, className, methodName]
        };
        this.tooltip = 'Click to open diagram preview';
    }

    applyRegistryInfo(info: RegistryMethodInfo, className: string, hasTrace: boolean = false): void {
        // Phase A: async label (registry is more reliable than source scan)
        if (info.isAsync && !this.label!.toString().includes('⚡')) {
            this.label = this.methodName + '⚡';
        }

        // Phase B — health icon
        // ✅ green  — diagram present + error types declared
        // ⚠️ amber  — diagram present, Bind/Gatekeeper nodes present, no declared error types
        // ⚪ grey   — diagram present, no fail-able paths (Terminal/Map/Tap only)
        // ❌ red    — no diagram
        const canFail = info.nodeKindFlags?.some(k => k === 'Bind' || k === 'Gatekeeper') ?? false;
        if (!info.hasDiagram) {
            this.iconPath = new vscode.ThemeIcon('circle-slash',
                new vscode.ThemeColor('testing.iconFailed'));
        } else if ((info.errorTypes?.length ?? 0) > 0) {
            this.iconPath = new vscode.ThemeIcon('pass-filled',
                new vscode.ThemeColor('testing.iconPassed'));
        } else if (canFail) {
            this.iconPath = new vscode.ThemeIcon('warning',
                new vscode.ThemeColor('testing.iconQueued'));
        } else {
            // No fail-able paths — Terminal/Map/Tap only
            this.iconPath = new vscode.ThemeIcon('pass-filled',
                new vscode.ThemeColor('testing.iconSkipped'));
        }

        // Phase B — error children (expand method node when errors present)
        if ((info.errorTypes?.length ?? 0) > 0) {
            this.errorChildren = info.errorTypes!.map(e => new ErrorNode(e));
            this.collapsibleState = vscode.TreeItemCollapsibleState.Collapsed;
        }

        // Phase B — description: returnType + nodeCount + error badge
        const errorBadge = (info.errorTypes?.length ?? 0) > 0
            ? ` · ${info.errorTypes!.length} error${info.errorTypes!.length > 1 ? 's' : ''}`
            : '';
        this.description = info.nodeCount !== undefined
            ? `${info.returnType} · ${info.nodeCount} nodes${errorBadge}`
            : `${info.returnType}${errorBadge}`;

        // Phase B — nodeCount for stats
        this.nodeCount = info.nodeCount ?? null;

        // Phase B — sourceLine (0-based) for goToSource
        this.sourceLine = info.sourceLine - 1;

        // pipelineId — FNV-1a hash for Live panel trace correlation
        this.pipelineId = info.pipelineId ?? null;

        // Phase B — tooltip: health state + full details
        const healthLine = !info.hasDiagram
            ? '\n\n❌ No diagram — build the project to generate it.'
            : (info.errorTypes?.length ?? 0) > 0
                ? ''
                : canFail
                    ? '\n\n⚠️ Pipeline can fail but no error types are declared in the registry.'
                    : '\n\n⚪ No fail-able paths detected (Terminal/Map/Tap only).';
        const kindLine  = info.nodeKindFlags?.length ? `\n\nKinds: ${info.nodeKindFlags.join(', ')}` : '';
        const errorLine = info.errorTypes?.length    ? `\n\nErrors: ${info.errorTypes.join(', ')}`   : '';
        this.tooltip = new vscode.MarkdownString(
            `**${this.methodName}**  \n→ \`${info.returnTypeFullName}\`` +
            (info.nodeCount !== undefined ? `  \n${info.nodeCount} nodes` : '') +
            healthLine + kindLine + errorLine
        );

        // Phase B — command: pass className + methodName directly so preview skips text parsing
        this.command = {
            command:   'reslava._previewMethod',
            title:     'Open Diagram Preview',
            arguments: [this.uri, info.sourceLine - 1, className, this.methodName]
        };

        // Debug inline button: only shown for instance-method classes with _Traced generated
        this.contextValue = hasTrace ? 'reslavaMethodTraceable' : 'reslavaMethod';
    }
}

export class LoadingPlaceholderItem extends vscode.TreeItem {
    constructor() {
        super('Loading pipelines\u2026', vscode.TreeItemCollapsibleState.None);
        this.iconPath     = new vscode.ThemeIcon('loading~spin');
        this.contextValue = 'reslavaLoading';
    }
}

export class EmptyStateItem extends vscode.TreeItem {
    constructor() {
        super('No pipelines \u2014 install REslava.Result.Flow', vscode.TreeItemCollapsibleState.None);
        this.description  = 'or REslava.ResultFlow';
        this.tooltip      = new vscode.MarkdownString(
            'Add `[ResultFlow]` to your pipeline methods and build the project.\n\n' +
            '**Track A** (REslava.Result users): `dotnet add package REslava.Result.Flow`\n\n' +
            '**Track B** (any Result library): `dotnet add package REslava.ResultFlow`\n\n' +
            '[See REslava.Result.Flow.Demo \u2192](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/REslava.Result.Flow.Demo)'
        );
        this.tooltip.isTrusted = true;
        this.iconPath     = new vscode.ThemeIcon('info');
        this.contextValue = 'reslavaEmpty';
        this.command      = {
            command:   'reslava.openDemo',
            title:     'Open Demo on GitHub',
            arguments: []
        };
    }
}

export class ErrorNode extends vscode.TreeItem {
    constructor(readonly errorTypeName: string) {
        super(errorTypeName, vscode.TreeItemCollapsibleState.None);
        this.iconPath     = new vscode.ThemeIcon('warning',
            new vscode.ThemeColor('testing.iconQueued'));  // amber
        this.contextValue = 'reslavaError';
        this.tooltip      = `Go to definition of ${errorTypeName}`;
        // Opens workspace symbol search pre-filtered to this type name
        this.command = {
            command:   'workbench.action.quickOpen',
            title:     'Go to Error Type',
            arguments: ['#' + errorTypeName]
        };
    }
}

// ─── Registry file walking & parsing ─────────────────────────────────────────

function findRegistryFiles(projectDir: string): Map<string, Map<string, RegistryMethodInfo>> {
    // Returns: className → (methodName → RegistryMethodInfo)
    const result = new Map<string, Map<string, RegistryMethodInfo>>();
    walkRegistryDir(path.join(projectDir, 'obj'), result);
    return result;
}

function walkRegistryDir(
    dir: string,
    result: Map<string, Map<string, RegistryMethodInfo>>
): void {
    let entries: fs.Dirent[];
    try { entries = fs.readdirSync(dir, { withFileTypes: true }); }
    catch { return; }

    for (const entry of entries) {
        const fullPath = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            walkRegistryDir(fullPath, result);
        } else if (entry.name.endsWith('_PipelineRegistry.g.cs')) {
            const className = entry.name.replace('_PipelineRegistry.g.cs', '');
            try {
                const content = fs.readFileSync(fullPath, 'utf8');
                result.set(className, parseRegistryContent(content));
            } catch { /* skip */ }
        }
    }
}

function parseRegistryContent(content: string): Map<string, RegistryMethodInfo> {
    const result   = new Map<string, RegistryMethodInfo>();
    const infoRegex = /public const string (\w+)_Info = "(.+?)";/g;
    let match: RegExpExecArray | null;

    while ((match = infoRegex.exec(content)) !== null) {
        const methodName = match[1];
        const rawJson    = match[2];
        try {
            // Unescape C# regular string escaping: \" → "  and  \\ → \
            const jsonStr = rawJson.replace(/\\"/g, '"').replace(/\\\\/g, '\\');
            result.set(methodName, JSON.parse(jsonStr) as RegistryMethodInfo);
        } catch { /* skip malformed */ }
    }

    return result;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

function truncateNamespace(ns: string): string {
    if (!ns) { return ''; }
    const parts = ns.split('.');
    return parts.length <= 2 ? ns : parts.slice(0, 2).join('.');
}

function findOwnerProject(
    csPath: string,
    projects: Array<{ fsPath: string; dir: string }>
): string | null {
    let best: { fsPath: string; dir: string } | null = null;
    const csLower = csPath.toLowerCase();
    for (const proj of projects) {
        const dirLower = proj.dir.toLowerCase();
        if (csLower.startsWith(dirLower + path.sep.toLowerCase()) ||
            csLower.startsWith(dirLower + '/')) {
            if (!best || proj.dir.length > best.dir.length) { best = proj; }
        }
    }
    return best ? best.fsPath : null;
}

function resolveMethodInfo(
    lines: string[],
    fromIndex: number
): { name: string; isAsync: boolean } | null {
    for (let i = fromIndex; i < Math.min(fromIndex + 5, lines.length); i++) {
        const m = lines[i].match(METHOD_DECL);
        if (m) {
            return { name: m[1], isAsync: /\basync\b/.test(lines[i]) };
        }
    }
    return null;
}
