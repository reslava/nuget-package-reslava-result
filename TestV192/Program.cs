using REslava.Result;

// STEP 1: Add the attribute to enable source generation
[assembly: REslava.Result.SourceGenerators.GenerateResultExtensions(
    Namespace = "TestProject.Generated",
    DefaultErrorStatusCode = 400)]

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

// Test Result<T> usage
Result<TestModel> success = Result<TestModel>.Ok(new TestModel { Id = 1, Name = "Test" });
Result<TestModel> failure = Result<TestModel>.Fail(new TestError("Not found"));

Console.WriteLine($"Success: {success.IsSuccess}");
Console.WriteLine($"Failure: {failure.IsFailed}");

public class TestModel 
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TestError : Error
{
    public TestError(string message) : base(message) { }
}
