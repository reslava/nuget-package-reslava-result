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

public abstract class Reason<TReason> : Reason
        where TReason : Reason<TReason>
{
    public Reason() : base() { }
    public Reason(string message) : base(message) { }

    public TReason WithMessage(string message) { Message = message; return (TReason)this; }
    public TReason WithTags(string key, object value) { Tags.Add(key, value); return (TReason)this; }
    public TReason WithTags(Dictionary<string, object> metadata)
    {
        foreach (var metadataItem in metadata)
        {
            Tags.Add(metadataItem.Key, metadataItem.Value);
        }

        return (TReason)this;
    }
}
