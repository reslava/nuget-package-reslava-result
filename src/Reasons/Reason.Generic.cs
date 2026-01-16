namespace REslava.Result;

// Generic class - adds fluent API
public abstract class Reason<TReason> : Reason
    where TReason : Reason<TReason>
{
    protected Reason(string message) : base(message) { }

    protected Reason(string message, Dictionary<string, object> tags)
        : base(message, tags) { }

    // Fluent methods that return TReason
    public TReason WithMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        { 
            throw new ArgumentException("Message cannot be empty", nameof(message));
        }   

        return CreateNew(message, GetTagsCopy());
    }

    public TReason WithTags(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
        { 
            throw new ArgumentException("Tag key cannot be empty", nameof(key));
        }

        var newTags = GetTagsCopy();
        newTags[key] = value;

        return CreateNew(Message, newTags);
    }

    public TReason WithTags(params (string key, object value)[] tags)
    {
        var newTags = GetTagsCopy();

        foreach (var (key, value) in tags)
        {
            if (string.IsNullOrWhiteSpace(key))
            { 
                throw new ArgumentException("Tag key cannot be empty");
            }

            newTags[key] = value;
        }

        return CreateNew(Message, newTags);
    }

    // Helper to get a copy of current tags
    private Dictionary<string, object> GetTagsCopy()
    {
        return new Dictionary<string, object>(Tags);
    }

    // Abstract factory method for derived classes
    protected abstract TReason CreateNew(string message, Dictionary<string, object> tags);
}
