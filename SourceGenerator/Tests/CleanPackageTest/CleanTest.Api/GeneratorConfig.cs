using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions(
    Namespace = "Generated.ResultExtensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    DefaultErrorStatusCode = 400
)]
