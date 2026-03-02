using Microsoft.CodeAnalysis;

namespace REslava.ResultFlow.Core.Interfaces
{
    internal interface IGeneratorOrchestrator
    {
        void Initialize(IncrementalGeneratorInitializationContext context);
    }
}
