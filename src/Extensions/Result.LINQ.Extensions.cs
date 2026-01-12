namespace REslava.Result;

public static class ResultLINQExtensions
{
  #region SelectMany
  public static Result<T> SelectMany<S, T>(
    this Result<S> source,
    Func<S, Result<T>> selector)
  {
    if(source.IsFailed)
    {
      return (Result<T>)(source as Result);
    }

    return selector(source.Value!); 
  } 
  public static Result<T> SelectMany<S, I, T>(
    this Result<S> source,
    Func<S, Result<I>> selector,
    Func<I, Result<T>> resultSelector)
  {
    if(source.IsFailed)
    {
      return (Result<T>)(source as Result);
    }
    var resultI = selector(source.Value!);

    if(resultI.IsFailed)
    {
      return (Result<T>)(resultI as Result);
    }
    return resultSelector(resultI.Value!); 
  }  

  public static async Task<Result<T>> SelectManyAsync<S, T>(
    this Result<S> source,
    Func<S, Task<Result<T>>> selector)
  {
    if(source.IsFailed)
    {
      return (Result<T>)(source as Result);
    }
    
    return await selector(source.Value!); 
  }

  public static async Task<Result<T>> SelectMany<S, I, T>(
    this Result<S> source,
    Func<S, Task<Result<I>>> selector,
    Func<I, Task<Result<T>>> resultSelector)
  {
    if(source.IsFailed)
    {
      return (Result<T>)(source as Result);
    }
    var resultI = await selector(source.Value!);

    if(resultI.IsFailed)
    {
      return (Result<T>)(resultI as Result);
    }
    return await resultSelector(resultI.Value!); 
  }
  public static async Task<Result<T>> SelectMany<S, I, T>(
    this Result<S> source,
    Func<S, Result<I>> selector,
    Func<I, Task<Result<T>>> resultSelector)
  {
    if(source.IsFailed)
    {
      return (Result<T>)(source as Result);
    }
    var resultI = selector(source.Value!);

    if(resultI.IsFailed)
    {
      return (Result<T>)(resultI as Result);
    }
    return await resultSelector(resultI.Value!); 
  }
  public static async Task<Result<T>> SelectMany<S, I, T>(
    this Result<S> source,
    Func<S, Task<Result<I>>> selector,
    Func<I, Result<T>> resultSelector)
  {
    if(source.IsFailed)
    {
      return (Result<T>)(source as Result);
    }
    var resultI = await selector(source.Value!);

    if(resultI.IsFailed)
    {
      return (Result<T>)(resultI as Result);
    }
    return resultSelector(resultI.Value!); 
  }
  #endregion

  public static Result<T> Select<S, T>(
    this Result<S> source,
    Func<S, T> selector)
  {
    var resultT = (Result<T>)(source as Result);
    if(source.IsFailed)
    {
      return (Result<T>)(source as Result);
    }
    
    return Result<T>.Ok(selector(source.Value!));
  }

  public static Result<S> Where<S>(
    this Result<S> source,
    Func<S, bool> predicate)
  {
    if(source.IsFailed)
    {
      return source;
    }
    
    return predicate(source.Value!) ? source : Result<S>.Fail("Predicate not satisfied");
  }
}