namespace REslava.Result;

public class Success : Reason<Success>, ISuccess
{
    public Success () : base () { }
    public Success (string message) : base (message) { }    
}