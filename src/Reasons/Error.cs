namespace REslava.Result;

public class Error : Reason<Error>, IError
{    
    public Error () : base () { }
    public Error (string message) : base (message) { }
}

