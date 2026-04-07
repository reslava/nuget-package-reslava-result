// Mirrors the JSON shape produced by REslava.Result.Observers.NodeTrace
export interface NodeTraceJson {
    nodeId: string;
    stepName: string;
    isSuccess: boolean;
    inputValue: string | null;
    outputValue: string | null;
    errorType: string | null;
    errorMessage: string | null;
    elapsedMs: number;
    nodeIndex: number;
}

// Mirrors the JSON shape produced by REslava.Result.Observers.PipelineTrace
export interface PipelineTraceJson {
    pipelineId: string;
    methodName: string;
    isSuccess: boolean;
    errorType: string | null;
    inputValue: string | null;
    outputValue: string | null;
    elapsedMs: number;
    startedAt: string;
    endedAt: string;
    nodes: NodeTraceJson[];
}

// Compact schema sent to the Claude API — all value strings truncated to MAX_VALUE_LENGTH
export interface FlowAIModel {
    method: string;
    isSuccess: boolean;
    inputs: string;
    output: string | null;
    errorType: string | null;
    errorMessage: string | null;
    failingNode: string | null;
    nodes: {
        name: string;
        isSuccess: boolean;
        elapsedMs: number;
        output: string | null;
    }[];
}

const MAX_VALUE_LENGTH = 500;

function trunc(s: string | null | undefined): string | null {
    if (!s) { return null; }
    return s.length > MAX_VALUE_LENGTH ? s.slice(0, MAX_VALUE_LENGTH) + '\u2026' : s;
}

/** Converts a full PipelineTrace JSON object to the compact FlowAIModel sent to Claude. */
export function buildFlowAIModel(trace: PipelineTraceJson): FlowAIModel {
    const failingNode = trace.nodes.find(n => !n.isSuccess) ?? null;
    return {
        method:       trace.methodName,
        isSuccess:    trace.isSuccess,
        inputs:       trunc(trace.inputValue) ?? '(unknown)',
        output:       trunc(trace.outputValue),
        errorType:    trace.errorType ?? failingNode?.errorType ?? null,
        errorMessage: failingNode?.errorMessage ?? null,
        failingNode:  failingNode?.stepName ?? null,
        nodes: trace.nodes.map(n => ({
            name:      n.stepName,
            isSuccess: n.isSuccess,
            elapsedMs: n.elapsedMs,
            output:    trunc(n.outputValue)
        }))
    };
}
