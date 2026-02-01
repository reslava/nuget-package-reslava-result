using Microsoft.CodeAnalysis;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult.Orchestration.Interfaces;

/// <summary>
/// Interface for orchestrating OneOf to IResult generation.
/// Following the same pattern as IOrchestrator from ResultToIResult.
/// </summary>
public interface IOneOfToIResultOrchestrator
{
    /// <summary>
    /// Initializes the generator pipeline.
    /// This is the main entry point following the ResultToIResult pattern.
    /// </summary>
    void Initialize(IncrementalGeneratorInitializationContext context);
}

/// <summary>
/// Represents a generated source file with its name and content.
/// </summary>
public class GeneratedSourceFile
{
    /// <summary>
    /// Gets the file name (without extension).
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the source code content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets the hint name for the source file.
    /// </summary>
    public string HintName => $"{FileName}.g.cs";
}
