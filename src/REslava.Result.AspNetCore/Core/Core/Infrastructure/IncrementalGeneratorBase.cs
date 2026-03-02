using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using REslava.Result.SourceGenerators.Core.Configuration;

namespace REslava.Result.SourceGenerators.Core.Infrastructure
{
    /// <summary>
    /// Base class for incremental source generators with configuration support.
    /// Provides common infrastructure for attribute discovery, configuration parsing,
    /// and code generation orchestration.
    /// </summary>
    /// <typeparam name="TConfig">The configuration type for this generator.</typeparam>
    public abstract class IncrementalGeneratorBase<TConfig> : IIncrementalGenerator
        where TConfig : GeneratorConfigurationBase, new()
    {
        /// <summary>
        /// The fully qualified name of the attribute that enables this generator.
        /// </summary>
        protected abstract string AttributeFullName { get; }

        /// <summary>
        /// The short name of the attribute (without namespace).
        /// </summary>
        protected abstract string AttributeShortName { get; }

        /// <summary>
        /// The name of the generated file (without extension).
        /// </summary>
        protected abstract string GeneratedFileName { get; }

        /// <summary>
        /// The source code for the attribute to be embedded in consuming projects.
        /// </summary>
        protected abstract string AttributeSourceCode { get; }

        /// <inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source first so it's available for compilation
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource($"{AttributeShortName}.g.cs",
                    SourceText.From(AttributeSourceCode, Encoding.UTF8));
            });

            // Find assemblies with the generator attribute
            var assemblyAttributes = context.CompilationProvider
                .Select((compilation, cancellationToken) =>
                {
                    var attributes = compilation.Assembly.GetAttributes();
                    var targetAttribute = attributes.FirstOrDefault(a =>
                        a.AttributeClass?.ToDisplayString() == AttributeFullName ||
                        a.AttributeClass?.Name == AttributeShortName);

                    if (targetAttribute == null)
                        return (compilation, (TConfig?)null);

                    // Parse configuration from attribute
                    var config = ParseConfiguration(targetAttribute);
                    
                    // Validate configuration
                    if (!config.Validate())
                        return (compilation, (TConfig?)null);

                    return (compilation, config);
                });

            // Generate the code
            context.RegisterSourceOutput(assemblyAttributes, (spc, data) =>
            {
                if (data == default) return;

                var compilation = data.Item1;
                var config = data.Item2;
                if (compilation == null || config == null) return;
                
                try
                {
                    var source = GenerateCode(compilation, config);
                    
                    spc.AddSource($"{GeneratedFileName}.g.cs",
                        SourceText.From(source, Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    // Report diagnostic if code generation fails
                    var descriptor = new DiagnosticDescriptor(
                        id: "RESLAVA001",
                        title: "Code generation failed",
                        messageFormat: "Failed to generate code: {0}",
                        category: "CodeGeneration",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true);

                    spc.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None, ex.Message));
                }
            });
        }

        /// <summary>
        /// Parses the configuration from the attribute data.
        /// Override this to parse generator-specific configuration properties.
        /// </summary>
        protected virtual TConfig ParseConfiguration(AttributeData attribute)
        {
            var config = new TConfig();
            
            // Parse common properties
            var namedArgs = attribute.NamedArguments;
            config.Namespace = namedArgs.GetStringValue("Namespace", config.Namespace) ?? config.Namespace;
            config.IncludeDiagnostics = namedArgs.GetBoolValue("IncludeDiagnostics", config.IncludeDiagnostics);
            config.IncludeDocumentation = namedArgs.GetBoolValue("IncludeDocumentation", config.IncludeDocumentation);

            // Allow derived classes to parse additional properties
            ParseAdditionalConfiguration(attribute, config);

            return config;
        }

        /// <summary>
        /// Override this to parse additional configuration properties specific to your generator.
        /// </summary>
        protected virtual void ParseAdditionalConfiguration(AttributeData attribute, TConfig config)
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Generates the source code for this generator.
        /// This is the main code generation method that derived classes must implement.
        /// </summary>
        /// <param name="compilation">The compilation context.</param>
        /// <param name="config">The parsed configuration.</param>
        /// <returns>The generated source code.</returns>
        protected abstract string GenerateCode(Compilation compilation, TConfig config);
    }
}
