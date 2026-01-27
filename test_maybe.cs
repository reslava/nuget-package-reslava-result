using REslava.Result.AdvancedPatterns;

public class TestMaybe
{
    public static void Main()
    {
        var maybe = Maybe<int>.Some(42);
        System.Console.WriteLine($"HasValue: {maybe.HasValue}");
        System.Console.WriteLine($"Value: {maybe.Value}");
        System.Console.WriteLine("Maybe<T> is working!");
    }
}
