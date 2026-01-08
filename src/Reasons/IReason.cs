namespace REslava.Result;

public interface IReason
{
    string Message { get; }
    Dictionary<string, object> Tags { get; }
}
