using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Core.Interfaces;
using REslava.Result.SourceGenerators.SmartEndpoints.Orchestration;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Attributes;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.CodeGeneration;

namespace REslava.Result.SourceGenerators.Generators.SmartEndpoints
{
    /// <summary>
    /// SmartEndpoints generator - automatically generates ASP.NET Core Minimal API endpoints
    /// from methods returning Result&lt;T&gt; or OneOf&lt;...&gt; types.
    /// Single Responsibility: Only delegates to orchestrator.
    /// </summary>
    [Generator]
    public class SmartEndpointsGenerator : IIncrementalGenerator
    {
        private readonly SmartEndpointsOrchestrator _orchestrator;
        // Static field to track constructor issues
        public static string? ConstructorError { get; private set; }

        public SmartEndpointsGenerator()
        {
            System.Diagnostics.Debug.WriteLine("🚀 SmartEndpointsGenerator CONSTRUCTOR called!");
            
            try
            {
                var autoGenerateEndpointsAttributeGenerator = new AutoGenerateEndpointsAttributeGenerator();
                var autoMapEndpointAttributeGenerator = new AutoMapEndpointAttributeGenerator();
                var smartEndpointExtensionGenerator = new SmartEndpointExtensionGenerator();
                
                _orchestrator = new SmartEndpointsOrchestrator();
                    // autoGenerateEndpointsAttributeGenerator,
                    // autoMapEndpointAttributeGenerator,
                    // smartEndpointExtensionGenerator);
                    
                System.Diagnostics.Debug.WriteLine("✅ SmartEndpointsGenerator constructor succeeded!");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SmartEndpointsGenerator constructor failed: {ex}");
                ConstructorError = ex.Message;
            }
        }

        /// <summary>
        /// Initializes the generator pipeline using orchestrator.
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🚀 SmartEndpointsGenerator.Initialize called!");
            
            try
            {
                _orchestrator.Initialize(context);
                System.Diagnostics.Debug.WriteLine("✅ SmartEndpointsGenerator initialized successfully!");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SmartEndpointsGenerator.Initialize failed: {ex}");
                ConstructorError = ex.Message;
            }
        }
    }
}
