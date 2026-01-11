namespace REslava.Result;

public abstract class Reason : IReason
{
    public string Message { get; protected set; }
    public Dictionary<string, object> Tags { get; }

    protected Reason()
    {
        Message = string.Empty;
        Tags = [];
    }
    public Reason(string message) : this()
    {
        Message = message;
    }

    public override string ToString()
    {
        var tagsString = Tags.Any() ?
            $" {nameof(Tags)}: {string.Join(", ", Tags)}" :
            string.Empty;

        return $"{GetType().Name}: {Message}{tagsString}";
    }
}
