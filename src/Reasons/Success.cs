namespace REslava.Result;

public class Success : Reason<Success>, ISuccess
{
    public Success() : base(string.Empty) { }
    public Success(string message) : base(message) { }

    private Success(string message, Dictionary<string, object> tags)
        : base(message, tags) { }

    protected override Success CreateNew(string message, Dictionary<string, object> tags)
    {
        return new Success(message, tags);
    }
}
