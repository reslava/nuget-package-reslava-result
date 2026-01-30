using REslava.Result.SourceGenerators.Core.Configuration;

namespace REslava.Result.SourceGenerators.Generators.ResultToIResult;

public class ResultToIResultConfig : GeneratorConfigurationBase
{
    /// <summary>
    /// Gets or sets whether to include error tags in the generated code.
    /// </summary>
    public bool IncludeErrorTags { get; set; } = true;

    /// <summary>
    /// Gets or sets custom error mappings in the format "ErrorType:StatusCode".
    /// </summary>
    public string[] CustomErrorMappings { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets whether to generate HTTP method extensions.
    /// </summary>
    public bool GenerateHttpMethodExtensions { get; set; } = true;

    /// <summary>
    /// Gets or sets the default HTTP status code for errors.
    /// </summary>
    public int DefaultErrorStatusCode { get; set; } = 400;

    /// <summary>
    /// Gets or sets whether to include detailed error information.
    /// </summary>
    public bool IncludeDetailedErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to generate async extension methods.
    /// </summary>
    public bool GenerateAsyncMethods { get; set; } = true;

    public override GeneratorConfigurationBase Clone()
    {
        return new ResultToIResultConfig
        {
            Namespace = Namespace,
            IncludeDiagnostics = IncludeDiagnostics,
            IncludeDocumentation = IncludeDocumentation,
            IncludeErrorTags = IncludeErrorTags,
            CustomErrorMappings = (string[])CustomErrorMappings.Clone(),
            GenerateHttpMethodExtensions = GenerateHttpMethodExtensions,
            DefaultErrorStatusCode = DefaultErrorStatusCode,
            IncludeDetailedErrors = IncludeDetailedErrors,
            GenerateAsyncMethods = GenerateAsyncMethods
        };
    }

    public override bool Validate()
    {
        if (!base.Validate())
            return false;

        if (DefaultErrorStatusCode < 100 || DefaultErrorStatusCode > 599)
            return false;

        return true;
    }
}
