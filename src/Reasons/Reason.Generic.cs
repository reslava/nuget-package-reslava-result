namespace REslava.Result;

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
