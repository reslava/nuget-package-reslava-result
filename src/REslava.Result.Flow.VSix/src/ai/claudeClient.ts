import * as https from 'https';
import * as cp from 'child_process';
import * as fs from 'fs';
import * as path from 'path';
import * as vscode from 'vscode';

const API_URL  = 'https://api.anthropic.com/v1/messages';
const API_VER  = '2023-06-01';
const AI_MODEL = 'claude-haiku-4-5-20251001';

export interface ClaudeCallOptions {
    maxTokens: number;
}

/**
 * Calls Claude using CLI-first auto-detection:
 *   1. Claude Code CLI (`claude -p`) — uses Pro subscription, no API key needed
 *   2. Direct Anthropic API  — used if `resultflow.anthropicApiKey` is set
 *
 * On CLI failure distinguishes "installed but not in PATH" from "not installed"
 * to give actionable error messages.
 */
export async function callClaude(prompt: string, options: ClaudeCallOptions): Promise<string> {
    // Path 1: Claude Code CLI (uses existing Pro subscription)
    try {
        return await _callViaCLI(prompt);
    } catch (cliErr) {
        // CLI not found or failed — fall back to direct API if key is configured
        const apiKey = vscode.workspace.getConfiguration()
            .get<string>('resultflow.anthropicApiKey', '').trim();
        if (apiKey) {
            return _callViaAPI(prompt, apiKey, options.maxTokens);
        }
        // Neither path available — surface the most actionable error
        throw new Error(
            'AI features require either:\n' +
            '• Claude Code CLI in PATH (uses your Pro subscription automatically)\n' +
            '• "resultflow.anthropicApiKey" set in VS Code Settings (search "anthropic")\n\n' +
            `CLI: ${cliErr instanceof Error ? cliErr.message : String(cliErr)}`
        );
    }
}

// ── Path 1: Claude Code CLI ──────────────────────────────────────────────────

function _callViaCLI(prompt: string): Promise<string> {
    return new Promise((resolve, reject) => {
        // shell:true resolves 'claude.cmd' on Windows via PATH automatically
        const proc = cp.spawn('claude', [
            '-p',
            '--output-format', 'text',
            '--input-format',  'text',
            '--no-session-persistence',
            '--model',          AI_MODEL,
            '--max-budget-usd', '0.10'
        ], { shell: true });

        let stdout = '';
        let stderr = '';
        proc.stdout.on('data', (c: Buffer) => { stdout += c.toString(); });
        proc.stderr.on('data', (c: Buffer) => { stderr += c.toString(); });

        proc.on('error', (err: NodeJS.ErrnoException) => {
            if (err.code === 'ENOENT') {
                reject(new Error(_buildNotFoundMessage()));
            } else {
                reject(err);
            }
        });

        proc.on('close', (code) => {
            if (code === 0) {
                resolve(stdout.trim());
            } else {
                reject(new Error(
                    `Claude Code CLI exited with code ${code}: ${stderr.slice(0, 200)}`
                ));
            }
        });

        // Prompt via stdin — avoids any shell quoting / escaping issues
        proc.stdin!.write(prompt, 'utf8');
        proc.stdin!.end();
    });
}

/**
 * Returns an actionable error message based on whether Claude Code is installed
 * (but not in PATH) or not installed at all.
 */
function _buildNotFoundMessage(): string {
    const installedAt = _findClaudeInstall();
    if (installedAt) {
        return (
            `Claude Code CLI is installed but not in PATH.\n` +
            `Found at: ${installedAt}\n` +
            `Add it to your system PATH, then restart VS Code.\n` +
            `Or set "resultflow.anthropicApiKey" in VS Code Settings as a fallback.`
        );
    }
    return (
        `Claude Code CLI not found.\n` +
        `Install it from https://claude.ai/code, then restart VS Code.\n` +
        `Or set "resultflow.anthropicApiKey" in VS Code Settings as a fallback.`
    );
}

/**
 * Checks known install locations for the Claude Code CLI binary.
 * Returns the first found path, or null if not found.
 */
function _findClaudeInstall(): string | null {
    // CLAUDE_CODE_ENTRYPOINT is set by Claude Code on its child processes
    const entrypoint = process.env['CLAUDE_CODE_ENTRYPOINT'];
    if (entrypoint && fs.existsSync(entrypoint)) {
        return entrypoint;
    }

    const candidates: string[] = process.platform === 'win32'
        ? [
            path.join(process.env['APPDATA'] ?? '',      'npm', 'claude.cmd'),
            path.join(process.env['LOCALAPPDATA'] ?? '',  'Programs', 'claude', 'claude.exe'),
            path.join(process.env['LOCALAPPDATA'] ?? '',  'Programs', 'claude', 'claude.cmd')
        ]
        : [
            '/usr/local/bin/claude',
            path.join(process.env['HOME'] ?? '', '.npm-global', 'bin', 'claude'),
            '/opt/homebrew/bin/claude',
            path.join(process.env['HOME'] ?? '', '.local', 'bin', 'claude')
        ];

    return candidates.find(p => p && fs.existsSync(p)) ?? null;
}

// ── Path 2: Direct Anthropic API ────────────────────────────────────────────

function _callViaAPI(prompt: string, apiKey: string, maxTokens: number): Promise<string> {
    const body = JSON.stringify({
        model:      AI_MODEL,
        max_tokens: maxTokens,
        messages:   [{ role: 'user', content: prompt }]
    });

    return new Promise<string>((resolve, reject) => {
        const req = https.request(
            API_URL,
            {
                method:  'POST',
                headers: {
                    'Content-Type':      'application/json',
                    'x-api-key':         apiKey,
                    'anthropic-version': API_VER,
                    'Content-Length':    Buffer.byteLength(body)
                }
            },
            (res) => {
                let raw = '';
                res.on('data', (chunk: Buffer) => { raw += chunk.toString(); });
                res.on('end', () => {
                    if (res.statusCode !== 200) {
                        reject(new Error(
                            `Anthropic API error ${res.statusCode}: ${raw.slice(0, 300)}`
                        ));
                        return;
                    }
                    try {
                        const json = JSON.parse(raw) as { content?: { text?: string }[] };
                        resolve(json.content?.[0]?.text ?? '');
                    } catch (err) {
                        reject(new Error(`Failed to parse Anthropic response: ${err}`));
                    }
                });
            }
        );
        req.on('error', reject);
        req.write(body);
        req.end();
    });
}
