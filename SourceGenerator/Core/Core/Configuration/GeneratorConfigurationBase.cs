namespace REslava.Result.SourceGenerators.Core.Configuration
{
    /// <summary>
    /// Base configuration class for source generators.
    /// Inherit from this to add generator-specific configuration properties.
    /// </summary>
    public abstract class GeneratorConfigurationBase : IGeneratorConfiguration
    {
        /// <inheritdoc />
        public string Namespace { get; set; } = "Generated";

        /// <inheritdoc />
        public bool IncludeDiagnostics { get; set; } = false;

        /// <inheritdoc />
        public bool IncludeDocumentation { get; set; } = true;

        /// <summary>
        /// Creates a copy of this configuration.
        /// Override this in derived classes to include additional properties.
        /// </summary>
        public virtual GeneratorConfigurationBase Clone()
        {
            return (GeneratorConfigurationBase)MemberwiseClone();
        }

        /// <summary>
        /// Validates the configuration.
        /// Override this to add custom validation logic.
        /// </summary>
        /// <returns>True if configuration is valid, false otherwise.</returns>
        public virtual bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Namespace))
                return false;

            return true;
        }
    }
}
