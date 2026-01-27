using REslava.Result;

namespace REslava.Result.SourceGenerators.Tests
{
    /// <summary>
    /// Shared test models for all test classes.
    /// </summary>
    public class TestUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestNotFoundError : Error
    {
        public TestNotFoundError(string message) : base(message) { }
    }

    public class TestValidationError : Error
    {
        public TestValidationError(string message) : base(message) { }
    }

    public class TestDuplicateError : Error
    {
        public TestDuplicateError(string message) : base(message) { }
    }

    public class TestUnauthorizedError : Error
    {
        public TestUnauthorizedError(string message) : base(message) { }
    }

    public class TestForbiddenError : Error
    {
        public TestForbiddenError(string message) : base(message) { }
    }
}
