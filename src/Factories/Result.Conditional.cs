namespace REslava.Result;

public static class ResultConditionalExtensions
{
  
  public static Result OkIf(bool condition, string errorMessage)
  {
    return condition ? Result.Ok() : Result.Fail(errorMessage);
  }  
  public static Result OkIf(bool condition, IError error)
  {
    return condition ? Result.Ok() : Result.Fail(error);
  }
  public static Result FailIf(bool condition, string errorMessage)
  {
    return condition ? Result.Fail(errorMessage) : Result.Ok();
  }  
  public static Result FailIf(bool condition, IError error)
  {
    return condition ? Result.Fail(error) : Result.Ok();
  }
}