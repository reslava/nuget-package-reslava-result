using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.OneOf.Models;
using REslava.Result.SourceGenerators.Core.OneOf.Utilities;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.CodeGeneration;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.CodeGeneration.Interfaces;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.TypeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.TypeAnalysis.Interfaces;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.Orchestration.Interfaces;
using System.Text;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult.Orchestration;

/// <summary>
/// Orchestrates the generation pipeline for OneOfToIResult.
/// Single Responsibility: Only coordinates type analysis and code generation.
/// Following the exact same pattern as ResultToIResultOrchestrator.
/// </summary>
public class OneOfToIResultOrchestrator : IOneOfToIResultOrchestrator
{
    private readonly IOneOfTypeAnalyzer _typeAnalyzer;
    private readonly IOneOfCodeGenerator _codeGenerator;

    public OneOfToIResultOrchestrator()
    {
        // Constructor injection - following ResultToIResult pattern
        _typeAnalyzer = new OneOfTypeAnalyzer();
        _codeGenerator = new OneOfExtensionGenerator();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Register code generation pipeline
        // Following exact same pattern as ResultToIResultOrchestrator
        var pipeline = context.CompilationProvider.Select((compilation, _) => compilation);

        context.RegisterSourceOutput(pipeline, (spc, compilation) =>
        {
            // Find OneOf types and generate extension methods
            var oneOfTypes = _typeAnalyzer.FindOneOfTypes(compilation);
            
            var generatedFiles = OrchestrateGeneration(compilation, new OneOfToIResultConfig());
            
            // Add each generated file
            foreach (var file in generatedFiles)
            {
                spc.AddSource(file.HintName, SourceText.From(file.Content, Encoding.UTF8));
            }
        });
    }

    /// <summary>
    /// Orchestrates the generation of OneOf extension methods.
    /// This method is used by the Initialize method and can also be used for testing.
    /// </summary>
    public IEnumerable<GeneratedSourceFile> OrchestrateGeneration(Compilation compilation, OneOfToIResultConfig config)
    {
        var generatedFiles = new List<GeneratedSourceFile>();

        // Find all OneOf types in the compilation
        var oneOfTypes = _typeAnalyzer.FindOneOfTypes(compilation);
        
        // Process each OneOf type
        foreach (var oneOfType in oneOfTypes)
        {
            if (!_typeAnalyzer.ShouldProcessType(oneOfType, compilation))
                continue;

            var generatedFile = ProcessOneOfType(oneOfType, compilation, config);
            if (generatedFile != null)
            {
                generatedFiles.Add(generatedFile);
            }
        }

        return generatedFiles;
    }

    /// <summary>
    /// Processes a single OneOf type and generates extension methods.
    /// </summary>
    private GeneratedSourceFile? ProcessOneOfType(
        INamedTypeSymbol oneOfType,
        Compilation compilation,
        OneOfToIResultConfig config)
    {
        // Analyze the OneOf type
        var oneOfTypeInfo = _typeAnalyzer.AnalyzeOneOfType(oneOfType, compilation);
        if (oneOfTypeInfo == null)
            return null;

        // Create mapping results for each type argument
        var mappingResults = CreateMappingResults(oneOfTypeInfo, config);
        if (mappingResults.Count == 0)
            return null;

        // Create generation context
        var context = new OneOfGenerationContext
        {
            Compilation = compilation,
            OneOfTypeInfo = oneOfTypeInfo,
            Config = config,
            MappingResults = mappingResults
        };

        // Generate the code
        var generatedCode = _codeGenerator.GenerateClassStructure(context);
        
        // Create the source file
        var fileName = GenerateFileName(oneOfTypeInfo);
        return new GeneratedSourceFile
        {
            FileName = fileName,
            Content = generatedCode
        };
    }

    /// <summary>
    /// Creates mapping results for each type argument in the OneOf.
    /// </summary>
    private IReadOnlyList<OneOfMappingResult> CreateMappingResults(
        OneOfTypeInfo oneOfTypeInfo,
        OneOfToIResultConfig config)
    {
        var mappings = new List<OneOfMappingResult>();
        var statusMapper = new OneOfHttpStatusCodeMapper();

        foreach (var typeArg in oneOfTypeInfo.TypeArguments)
        {
            var typeName = OneOfTypeHelper.GetFullTypeName(typeArg);
            var cleanTypeName = OneOfTypeHelper.GetCleanTypeName(typeArg);
            var statusCode = statusMapper.DetermineStatusCode(typeName);
            var responseType = statusMapper.DetermineResponseType(typeName, statusCode);
            var isErrorType = statusMapper.IsErrorType(typeName);

            var mapping = new OneOfMappingResult
            {
                TypeName = typeName,
                CleanTypeName = cleanTypeName,
                StatusCode = statusCode,
                ResponseType = responseType,
                IsErrorType = isErrorType,
                IncludeProblemDetails = config.EnableProblemDetails && isErrorType
            };

            mappings.Add(mapping);
        }

        return mappings;
    }

    /// <summary>
    /// Generates a unique file name for the OneOf type.
    /// </summary>
    private static string GenerateFileName(OneOfTypeInfo oneOfTypeInfo)
    {
        // Create a safe file name based on the OneOf type
        var typeArgs = oneOfTypeInfo.TypeArguments
            .Select(OneOfTypeHelper.GetCleanTypeName)
            .Select(name => name.Replace("<", "_").Replace(">", "_").Replace(",", "_"))
            .ToArray();

        // Add timestamp to ensure uniqueness
        var timestamp = DateTime.UtcNow.Ticks.ToString("x");
        var fileName = $"OneOf_{string.Join("_", typeArgs)}_Extensions_{timestamp}";
        
        // Ensure file name is valid
        var invalidChars = Path.GetInvalidFileNameChars();
        fileName = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());

        return fileName;
    }
}
