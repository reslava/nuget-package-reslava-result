namespace REslava.Result.SourceGenerators.Core.Configuration
{
    /// <summary>
    /// Base interface for all generator configurations.
    /// Provides common properties that all generators should support.
    /// </summary>
    public interface IGeneratorConfiguration
    {
        /// <summary>
        /// The namespace for generated code.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Whether to include detailed diagnostic comments in generated code.
        /// </summary>
        bool IncludeDiagnostics { get; }

        /// <summary>
        /// Whether to include XML documentation comments in generated code.
        /// </summary>
        bool IncludeDocumentation { get; }
    }
}
