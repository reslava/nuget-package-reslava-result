namespace REslava.Result;

public class Error : Reason<Error>, IError
{
    public Error() : base(string.Empty) { }
    public Error(string message) : base(message) { }

    private Error(string message, Dictionary<string, object> tags)
        : base(message, tags) { }

    protected override Error CreateNew(string message, Dictionary<string, object> tags)
    {
        return new Error(message, tags);
    }
}
