namespace REslava.Result;

public interface IReason
{
    string Message { get; }
    IReadOnlyDictionary<string, object> Tags { get; }
}
