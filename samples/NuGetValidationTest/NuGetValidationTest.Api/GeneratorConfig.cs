using REslava.Result.SourceGenerators;

// Configure the SOLID architecture source generator
[assembly: GenerateResultExtensions(
    Namespace = "Generated.ResultExtensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    DefaultErrorStatusCode = 400
)]
