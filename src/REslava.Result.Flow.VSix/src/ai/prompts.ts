import { FlowAIModel } from './FlowAIModel';

/**
 * Builds the ExplainFailure prompt from a compact FlowAIModel.
 * Output: 3–5 plain-English sentences naming the step, error type, and likely cause.
 */
export function buildExplainFailurePrompt(model: FlowAIModel): string {
    const successfulNodes = model.nodes
        .filter(n => n.isSuccess)
        .map(n => `  ✓ ${n.name} (${n.elapsedMs}ms)`)
        .join('\n');

    return `You are a pipeline debugger. Explain why this Result pipeline failed in plain English.

Pipeline: ${model.method}
Input: ${model.inputs}
Failing step: ${model.failingNode ?? 'unknown'}
Error type: ${model.errorType ?? 'unknown'}
Error message: ${model.errorMessage ?? 'unknown'}

Nodes executed before failure:
${successfulNodes || '  (none — failed at first step)'}

Rules:
- 3–5 sentences maximum
- Be specific: name the step, error type, and likely cause
- Do NOT suggest fixes unless the cause is obvious from the data
- Do NOT repeat information already visible in the trace`;
}


/**
 * Builds the GenerateTest prompt from a compact FlowAIModel.
 * Output: a single MSTest method body (no class, no usings).
 */
export function buildGenerateTestPrompt(model: FlowAIModel): string {
    const outcome   = model.isSuccess ? 'SUCCESS' : 'FAILURE';
    const shortName = model.method.split('.').pop() ?? model.method;
    const verb      = model.isSuccess ? 'Succeed' : 'Fail';

    const outcomeBlock = model.isSuccess
        ? `Output: ${model.output ?? '(none)'}`
        : [
            `Error type: ${model.errorType ?? 'unknown'}`,
            `Error message: ${model.errorMessage ?? 'unknown'}`,
            `Failing step: ${model.failingNode ?? 'unknown'}`
        ].join('\n');

    const nodesSummary = model.nodes
        .map((n, i) => `  ${i + 1}. ${n.name} — ${n.isSuccess ? 'ok' : 'FAIL'} (${n.elapsedMs}ms)`)
        .join('\n');

    return `You are a C# test generator. Generate a single MSTest unit test for this pipeline execution.

Pipeline: ${model.method}
Outcome: ${outcome}
Inputs: ${model.inputs}
${outcomeBlock}

Nodes executed (in order):
${nodesSummary}

Rules:
- Use MSTest ([TestClass], [TestMethod])
- Use FluentAssertions (.Should())
- Test method name: ${shortName}_${outcome}_Should${verb}
- Include only what you know from the data above — do not invent values
- For unknown namespaces, leave a // TODO: add using comment
- Output ONLY the [TestMethod] method — no class declaration, no using statements`;
}
