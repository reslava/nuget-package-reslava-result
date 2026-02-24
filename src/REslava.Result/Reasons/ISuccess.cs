namespace REslava.Result;

/// <summary>
/// Marker interface for success reasons. Implement to attach structured metadata
/// to a successful result without changing its value.
/// </summary>
/// <seealso cref="Success"/>
public interface ISuccess : IReason { }
