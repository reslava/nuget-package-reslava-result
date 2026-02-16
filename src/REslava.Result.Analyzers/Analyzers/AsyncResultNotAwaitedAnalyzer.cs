using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL1004: Warns when a method returning Task&lt;Result&lt;T&gt;&gt; is called
    /// without await in an async method, causing the Task to be assigned instead
    /// of the actual Result.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncResultNotAwaitedAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL1004_AsyncResultNotAwaited);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var resultType = compilationContext.Compilation
                    .GetTypeByMetadataName("REslava.Result.Result`1");

                if (resultType is null)
                    return;

                var taskType = compilationContext.Compilation
                    .GetTypeByMetadataName("System.Threading.Tasks.Task`1");

                if (taskType is null)
                    return;

                compilationContext.RegisterSyntaxNodeAction(
                    ctx => AnalyzeLocalDeclaration(ctx, resultType, taskType),
                    SyntaxKind.LocalDeclarationStatement);

                compilationContext.RegisterSyntaxNodeAction(
                    ctx => AnalyzeAssignment(ctx, resultType, taskType),
                    SyntaxKind.SimpleAssignmentExpression);
            });
        }

        private static void AnalyzeLocalDeclaration(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol resultType,
            INamedTypeSymbol taskType)
        {
            var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

            foreach (var variable in localDeclaration.Declaration.Variables)
            {
                if (variable.Initializer?.Value is not { } initializer)
                    continue;

                // Skip if the initializer is already an await expression
                if (initializer is AwaitExpressionSyntax)
                    continue;

                // Check if the initializer returns Task<Result<T>>
                var typeInfo = context.SemanticModel.GetTypeInfo(initializer, context.CancellationToken);
                if (!IsTaskOfResult(typeInfo.Type as INamedTypeSymbol, resultType, taskType))
                    continue;

                // Only warn in async methods
                if (!IsInAsyncMethod(context.Node))
                    continue;

                // Skip if the declared type is explicitly Task<...> (intentional)
                if (IsExplicitTaskType(localDeclaration.Declaration.Type, context.SemanticModel, taskType, context.CancellationToken))
                    continue;

                var methodName = GetMethodName(initializer);

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.RESL1004_AsyncResultNotAwaited,
                        initializer.GetLocation(),
                        methodName ?? "expression"));
            }
        }

        private static void AnalyzeAssignment(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol resultType,
            INamedTypeSymbol taskType)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            var rhs = assignment.Right;

            // Skip if the RHS is already an await expression
            if (rhs is AwaitExpressionSyntax)
                return;

            // Check if the RHS returns Task<Result<T>>
            var typeInfo = context.SemanticModel.GetTypeInfo(rhs, context.CancellationToken);
            if (!IsTaskOfResult(typeInfo.Type as INamedTypeSymbol, resultType, taskType))
                return;

            // Only warn in async methods
            if (!IsInAsyncMethod(context.Node))
                return;

            var methodName = GetMethodName(rhs);

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptors.RESL1004_AsyncResultNotAwaited,
                    rhs.GetLocation(),
                    methodName ?? "expression"));
        }

        private static bool IsTaskOfResult(
            INamedTypeSymbol? type,
            INamedTypeSymbol resultType,
            INamedTypeSymbol taskType)
        {
            if (type is null || !type.IsGenericType)
                return false;

            if (!SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, taskType))
                return false;

            if (type.TypeArguments.Length != 1)
                return false;

            var innerType = type.TypeArguments[0] as INamedTypeSymbol;
            if (innerType is null || !innerType.IsGenericType)
                return false;

            return SymbolEqualityComparer.Default.Equals(innerType.OriginalDefinition, resultType);
        }

        private static bool IsInAsyncMethod(SyntaxNode node)
        {
            var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (method is not null)
                return method.Modifiers.Any(SyntaxKind.AsyncKeyword);

            var localFunc = node.FirstAncestorOrSelf<LocalFunctionStatementSyntax>();
            if (localFunc is not null)
                return localFunc.Modifiers.Any(SyntaxKind.AsyncKeyword);

            var lambda = node.FirstAncestorOrSelf<LambdaExpressionSyntax>();
            if (lambda is not null)
                return lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);

            return false;
        }

        private static bool IsExplicitTaskType(
            TypeSyntax typeSyntax,
            SemanticModel semanticModel,
            INamedTypeSymbol taskType,
            CancellationToken cancellationToken)
        {
            // var/implicit → not explicit Task type
            if (typeSyntax is IdentifierNameSyntax { Identifier.Text: "var" })
                return false;

            var typeInfo = semanticModel.GetTypeInfo(typeSyntax, cancellationToken);
            if (typeInfo.Type is INamedTypeSymbol declaredType
                && declaredType.IsGenericType
                && SymbolEqualityComparer.Default.Equals(declaredType.OriginalDefinition, taskType))
                return true;

            return false;
        }

        private static string? GetMethodName(ExpressionSyntax expression)
        {
            return expression switch
            {
                InvocationExpressionSyntax invocation => invocation.Expression switch
                {
                    IdentifierNameSyntax id => id.Identifier.Text,
                    MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                    _ => null
                },
                _ => null
            };
        }
    }
}
