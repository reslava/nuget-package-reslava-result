namespace REslava.Result.SourceGenerators.SmartEndpoints.Models;

/// <summary>
/// SmartEndpoints generation mode for controlling output behavior
/// </summary>
internal enum GenerationMode
{
    /// <summary>
    /// Cache mode: In-memory generation only (Release)
    /// </summary>
    Cache,
    
    /// <summary>
    /// File mode: Write generated files to disk (Debug)
    /// </summary>
    File
}
