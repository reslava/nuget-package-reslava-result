using Microsoft.CodeAnalysis;

namespace REslava.Result.SourceGenerators.Core.Utilities
{
    /// <summary>
    /// Utilities for creating and reporting diagnostics in source generators.
    /// Provides predefined diagnostic descriptors and helper methods.
    /// </summary>
    public static class DiagnosticUtilities
    {
        // Diagnostic IDs
        private const string CategoryGeneral = "SourceGenerator";
        private const string CategoryConfiguration = "Configuration";
        private const string CategoryCodeGeneration = "CodeGeneration";

        /// <summary>
        /// Creates a diagnostic descriptor for a general error.
        /// </summary>
        public static DiagnosticDescriptor CreateError(
            string id,
            string title,
            string messageFormat,
            string category = CategoryGeneral)
        {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);
        }

        /// <summary>
        /// Creates a diagnostic descriptor for a warning.
        /// </summary>
        public static DiagnosticDescriptor CreateWarning(
            string id,
            string title,
            string messageFormat,
            string category = CategoryGeneral)
        {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);
        }

        /// <summary>
        /// Creates a diagnostic descriptor for informational messages.
        /// </summary>
        public static DiagnosticDescriptor CreateInfo(
            string id,
            string title,
            string messageFormat,
            string category = CategoryGeneral)
        {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);
        }

        // Common Diagnostic Descriptors

        /// <summary>
        /// Configuration validation failed.
        /// </summary>
        public static readonly DiagnosticDescriptor ConfigurationError = CreateError(
            id: "RESLAVA001",
            title: "Configuration Error",
            messageFormat: "Configuration validation failed: {0}",
            category: CategoryConfiguration);

        /// <summary>
        /// Code generation failed.
        /// </summary>
        public static readonly DiagnosticDescriptor CodeGenerationError = CreateError(
            id: "RESLAVA002",
            title: "Code Generation Failed",
            messageFormat: "Failed to generate code: {0}",
            category: CategoryCodeGeneration);

        /// <summary>
        /// Attribute not found.
        /// </summary>
        public static readonly DiagnosticDescriptor AttributeNotFound = CreateWarning(
            id: "RESLAVA003",
            title: "Attribute Not Found",
            messageFormat: "Generator attribute not found in assembly",
            category: CategoryConfiguration);

        /// <summary>
        /// Invalid attribute usage.
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidAttributeUsage = CreateError(
            id: "RESLAVA004",
            title: "Invalid Attribute Usage",
            messageFormat: "Invalid attribute usage: {0}",
            category: CategoryConfiguration);

        /// <summary>
        /// Helper method to report a diagnostic.
        /// </summary>
        public static void Report(
            SourceProductionContext context,
            DiagnosticDescriptor descriptor,
            Location? location = null,
            params object[] messageArgs)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(descriptor, location ?? Location.None, messageArgs));
        }

        /// <summary>
        /// Reports a configuration error.
        /// </summary>
        public static void ReportConfigurationError(
            SourceProductionContext context,
            string message,
            Location? location = null)
        {
            Report(context, ConfigurationError, location, message);
        }

        /// <summary>
        /// Reports a code generation error.
        /// </summary>
        public static void ReportCodeGenerationError(
            SourceProductionContext context,
            string message,
            Location? location = null)
        {
            Report(context, CodeGenerationError, location, message);
        }

        /// <summary>
        /// Reports an invalid attribute usage error.
        /// </summary>
        public static void ReportInvalidAttributeUsage(
            SourceProductionContext context,
            string message,
            Location? location = null)
        {
            Report(context, InvalidAttributeUsage, location, message);
        }
    }
}
