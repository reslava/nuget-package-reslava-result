import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';

const RESULT_FLOW_ATTR = /^\s*\[ResultFlow[(\]]/;
const CLASS_DECL       = /(?:^|[\w\s]*?)class\s+(\w+)(?:\s*[:({<]|\s*$)/;
const METHOD_DECL      = /(?:public|private|protected|internal|async|static|\s)+\S+\s+(\w+)\s*[(<]/;

export type FlowTreeNode = ProjectNode | ClassNode | MethodNode;

interface RegistryMethodInfo {
    sourceLine:         number;
    returnType:         string;
    returnTypeFullName: string;
    isAsync:            boolean;
    hasDiagram:         boolean;
    nodeCount?:         number;
    errorTypes?:        string[];
    nodeKindFlags?:     string[];
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

    getTreeItem(node: FlowTreeNode): vscode.TreeItem { return node; }

    getChildren(node?: FlowTreeNode): FlowTreeNode[] {
        if (!node) {
            return [...this.cache.values()]
                .map(e => e.node)
                .filter(p => p.classes.length > 0)
                .sort((a, b) => a.projectName.localeCompare(b.projectName));
        }
        if (node instanceof ProjectNode) { return node.classes; }
        if (node instanceof ClassNode)   { return node.methods; }
        return [];
    }

    async scanWorkspace(): Promise<void> {
        this.cache.clear();
        this.fileToProject.clear();

        const csprojUris = await vscode.workspace.findFiles('**/*.csproj', '{**/obj/**,**/bin/**}');
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

        this._onDidChangeTreeData.fire();
    }

    refresh(uri?: vscode.Uri): void {
        if (uri) {
            const projectPath = this.fileToProject.get(uri.fsPath);
            if (projectPath) {
                this.scanFileIntoProject(uri.fsPath, projectPath);
                const entry = this.cache.get(projectPath);
                if (entry) { this.enrichFromRegistry(projectPath, entry); }
            }
        }
        this._onDidChangeTreeData.fire();
    }

    // Phase D: stats for treeView.message
    getStats(): { projects: number; pipelines: number; nodes: number } {
        let pipelines = 0;
        let nodes     = 0;
        for (const entry of this.cache.values()) {
            for (const cls of entry.node.classes) {
                pipelines += cls.methods.length;
                for (const m of cls.methods) { nodes += m.nodeCount ?? 0; }
            }
        }
        return { projects: this.cache.size, pipelines, nodes };
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
                            new MethodNode(resolved.name, vscode.Uri.file(filePath), i, resolved.isAsync)
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
            for (const method of cls.methods) {
                const info = methodInfoMap.get(method.methodName);
                if (info) { method.applyRegistryInfo(info); }
            }
        }
    }
}

// ─── Tree items ───────────────────────────────────────────────────────────────

export class ProjectNode extends vscode.TreeItem {
    classes: ClassNode[] = [];
    constructor(
        readonly projectName: string,
        readonly projectPath: string,
        relDir: string
    ) {
        super(projectName, vscode.TreeItemCollapsibleState.Expanded);
        this.description  = relDir;
        this.contextValue = 'reslavaProject';
        this.iconPath     = new vscode.ThemeIcon('package');
    }

    setHasRegistry(has: boolean): void {
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

export class ClassNode extends vscode.TreeItem {
    readonly methods: MethodNode[] = [];
    constructor(
        readonly className: string,
        readonly filePath: string,
        relativePath: string
    ) {
        super(className, vscode.TreeItemCollapsibleState.Expanded);
        this.description = relativePath;
        this.iconPath    = new vscode.ThemeIcon('symbol-class');
    }
}

export class MethodNode extends vscode.TreeItem {
    nodeCount: number | null = null;

    constructor(
        readonly methodName: string,
        readonly uri: vscode.Uri,
        lineNumber: number,
        isAsync: boolean = false
    ) {
        super(methodName + (isAsync ? '⚡' : ''), vscode.TreeItemCollapsibleState.None);
        this.iconPath = new vscode.ThemeIcon('symbol-method');
        this.command  = {
            command:   'reslava._previewMethod',
            title:     'Open Diagram Preview',
            arguments: [uri, lineNumber]
        };
        this.tooltip = 'Click to open diagram preview';
    }

    applyRegistryInfo(info: RegistryMethodInfo): void {
        // Phase A: async label (registry is more reliable than source scan)
        if (info.isAsync && !this.label!.toString().includes('⚡')) {
            this.label = this.methodName + '⚡';
        }

        // Phase B — icon by hasDiagram
        this.iconPath = info.hasDiagram
            ? new vscode.ThemeIcon('pass-filled', new vscode.ThemeColor('testing.iconPassed'))
            : new vscode.ThemeIcon('circle-outline');

        // Phase B — description: returnType + nodeCount
        this.description = info.nodeCount !== undefined
            ? `${info.returnType} · ${info.nodeCount} nodes`
            : info.returnType;

        // Phase B — nodeCount for stats
        this.nodeCount = info.nodeCount ?? null;

        // Phase B — tooltip: full details + errorTypes
        const kindLine  = info.nodeKindFlags?.length  ? `\n\nKinds: ${info.nodeKindFlags.join(', ')}`  : '';
        const errorLine = info.errorTypes?.length     ? `\n\nErrors: ${info.errorTypes.join(', ')}` : '';
        this.tooltip = new vscode.MarkdownString(
            `**${this.methodName}**  \n→ \`${info.returnTypeFullName}\`` +
            (info.nodeCount !== undefined ? `  \n${info.nodeCount} nodes` : '') +
            kindLine + errorLine
        );

        // Phase B — command: use sourceLine from registry (1-indexed → 0-based)
        this.command = {
            command:   'reslava._previewMethod',
            title:     'Open Diagram Preview',
            arguments: [this.uri, info.sourceLine - 1]
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
