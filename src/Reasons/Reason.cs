using System.Diagnostics;

namespace REslava.Result;

// Base class - defines the contract
public abstract class Reason : IReason
{
    private readonly Dictionary<string, object> _tags;

    public string Message { get; protected set; }
    public IReadOnlyDictionary<string, object> Tags => _tags;

    protected Reason(string message)
    {
        Message = message ?? string.Empty;
        _tags = [];
    }

    protected Reason(string message, Dictionary<string, object> tags)
    {
        Message = message ?? string.Empty;
        _tags = new Dictionary<string, object>(tags); // Defensive copy
    }
}

    //public override string ToString()
    //{
    //    var tagsString = Tags.Any() ?
    //        $" {nameof(Tags)}: {string.Join(", ", Tags)}" :
    //        string.Empty;

    //    return $"{GetType().Name}: {Message}{tagsString}";
    //}
