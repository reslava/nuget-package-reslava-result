namespace REslava.Result;

public partial class Result 
{
  /// <summary>
  /// Merges multiple results into a single result. If any result is failed, returns a failed result with all errors.
  /// </summary>
  public static Result Merge(IEnumerable<Result> results)
  {
    return Result.Ok().WithReasons(results.SelectMany(r => r.Reasons));  
  }  
  public static Result Merge(params Result[] results)
  {
    return Result.Ok().WithReasons(results.SelectMany(r => r.Reasons));  
  }
}