using REslava.Result.AdvancedPatterns;
using OneOfTest.Api.Models;

// Simple test class to trigger generator
public class TestGenerator
{
    // This should trigger T1,T2 extension generation (working)
    public REslava.Result.AdvancedPatterns.OneOf<UserNotFoundError, User> TestT1T2()
    {
        return new UserNotFoundError(0);
    }

    // This should trigger T1,T2,T3 extension generation (not working)
    public REslava.Result.AdvancedPatterns.OneOf<ValidationError, UserNotFoundError, User> TestT1T2T3()
    {
        return new ValidationError("Test", "Test error");
    }
}
