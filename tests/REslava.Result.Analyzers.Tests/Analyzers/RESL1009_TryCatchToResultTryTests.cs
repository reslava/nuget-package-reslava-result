using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.CodeFixes;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.Analyzers;

[TestClass]
public class RESL1009_TryCatchToResultTryTests
{
    // Stub that includes Try / TryAsync so the fixed code compiles
    private const string ResultStub = @"
using System;
using System.Threading.Tasks;
namespace REslava.Result
{
    public interface IError { }
    public class Error : IError { public Error(string msg) { } }
    public class ExceptionError : IError { public ExceptionError(Exception ex) { } }
    public class Result<T>
    {
        public bool IsSuccess => true;
        public bool IsFailure => false;
        public T Value => default!;
        public static Result<T> Ok(T value) => new Result<T>();
        public static Result<T> Fail(string error) => new Result<T>();
        public static Result<T> Fail(IError error) => new Result<T>();
        public static Result<T> Try(Func<T> action) => new Result<T>();
        public static Result<T> Try(Func<T> action, Func<Exception, IError> handler) => new Result<T>();
        public static Task<Result<T>> TryAsync(Func<Task<T>> action) => Task.FromResult(new Result<T>());
        public static Task<Result<T>> TryAsync(Func<Task<T>> action, Func<Exception, IError> handler) => Task.FromResult(new Result<T>());
        public static implicit operator Result<T>(T value) => Ok(value);
    }
}";

    private CSharpAnalyzerTest<TryCatchToResultTryAnalyzer, DefaultVerifier> CreateAnalyzerTest(string code)
    {
        var test = new CSharpAnalyzerTest<TryCatchToResultTryAnalyzer, DefaultVerifier>
        {
            TestCode = code,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        test.TestState.Sources.Add(("ResultStub.cs", ResultStub));
        return test;
    }

    private CSharpCodeFixTest<TryCatchToResultTryAnalyzer, TryCatchToResultTryCodeFixProvider, DefaultVerifier>
        CreateCodeFixTest(string code, string fixedCode, int actionIndex = 0)
    {
        var test = new CSharpCodeFixTest<TryCatchToResultTryAnalyzer, TryCatchToResultTryCodeFixProvider, DefaultVerifier>
        {
            TestCode = code,
            FixedCode = fixedCode,
            CodeActionIndex = actionIndex,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        test.TestState.Sources.Add(("ResultStub.cs", ResultStub));
        test.FixedState.Sources.Add(("ResultStub.cs", ResultStub));
        return test;
    }

    // ── Fires ──────────────────────────────────────────────────────────────

    // 1. Simple sync method — try { return T } catch (Exception) { return Fail(ExceptionError) }
    [TestMethod]
    public async Task Fires_OnSimpleSyncTryCatch_WithExceptionError()
    {
        var test = CreateAnalyzerTest(@"
using System;
using REslava.Result;
class C
{
    Result<int> GetUser(int id)
    {
        {|RESL1009:try|}
        {
            return id + 1;
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new ExceptionError(ex));
        }
    }
}");
        await test.RunAsync();
    }

    // 2. Async method — try { return await expr } catch (Exception) { return Fail(...) }
    [TestMethod]
    public async Task Fires_OnAsyncTryCatch_WithAwait()
    {
        var test = CreateAnalyzerTest(@"
using System;
using System.Threading.Tasks;
using REslava.Result;
class C
{
    async Task<Result<int>> GetAsync(int id)
    {
        {|RESL1009:try|}
        {
            return await Task.FromResult(id + 1);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new ExceptionError(ex));
        }
    }
}");
        await test.RunAsync();
    }

    // 3. Custom error in catch body — still fires
    [TestMethod]
    public async Task Fires_OnCustomErrorInCatch()
    {
        var test = CreateAnalyzerTest(@"
using System;
using REslava.Result;
class C
{
    Result<int> GetUser(int id)
    {
        {|RESL1009:try|}
        {
            return id + 1;
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new Error($""Failed: {ex.Message}""));
        }
    }
}");
        await test.RunAsync();
    }

    // 4. Fires when try body is a simple expression
    [TestMethod]
    public async Task Fires_OnPublicMethodReturningResult()
    {
        var test = CreateAnalyzerTest(@"
using System;
using REslava.Result;
class C
{
    public Result<string> Parse(string s)
    {
        {|RESL1009:try|}
        {
            return s.Trim();
        }
        catch (Exception ex)
        {
            return Result<string>.Fail(new ExceptionError(ex));
        }
    }
}");
        await test.RunAsync();
    }

    // ── Does NOT fire ──────────────────────────────────────────────────────

    // 5. Specific exception type → no diagnostic
    [TestMethod]
    public async Task NoFire_OnSpecificExceptionType()
    {
        var test = CreateAnalyzerTest(@"
using System;
using REslava.Result;
class SqlException : Exception { }
class C
{
    Result<int> GetUser(int id)
    {
        try
        {
            return id + 1;
        }
        catch (SqlException ex)
        {
            return Result<int>.Fail(new ExceptionError(ex));
        }
    }
}");
        await test.RunAsync();
    }

    // 6. Try body already returns Result<T> → no diagnostic (use Catch() instead)
    [TestMethod]
    public async Task NoFire_WhenTryBodyReturnsResult()
    {
        var test = CreateAnalyzerTest(@"
using System;
using REslava.Result;
class C
{
    Result<int> GetUser(int id)
    {
        try
        {
            return Result<int>.Ok(id + 1);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new ExceptionError(ex));
        }
    }
}");
        await test.RunAsync();
    }

    // 7. try/finally with no catch → no diagnostic
    [TestMethod]
    public async Task NoFire_OnTryFinally_NoCatch()
    {
        var test = CreateAnalyzerTest(@"
using System;
using REslava.Result;
class C
{
    Result<int> GetUser(int id)
    {
        try
        {
            return id + 1;
        }
        finally
        {
            // cleanup
        }
    }
}");
        await test.RunAsync();
    }

    // 8. Method returns string, not Result<T> → no diagnostic
    [TestMethod]
    public async Task NoFire_WhenMethodDoesNotReturnResult()
    {
        var test = CreateAnalyzerTest(@"
using System;
class C
{
    string GetUser(int id)
    {
        try
        {
            return id.ToString();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}");
        await test.RunAsync();
    }

    // ── Code fix ───────────────────────────────────────────────────────────

    // 9. Fix A: Result<T>.Try(() => expr) — simple ExceptionError case
    [TestMethod]
    public async Task FixA_ReplacesEntireMethod_WithTryExpression()
    {
        var testCode = @"
using System;
using REslava.Result;
class C
{
    Result<int> GetUser(int id)
    {
        {|RESL1009:try|}
        {
            return id + 1;
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new ExceptionError(ex));
        }
    }
}";

        var fixedCode = @"
using System;
using REslava.Result;
class C
{
    Result<int> GetUser(int id) =>
        Result<int>.Try(() => id + 1);
}";

        var test = CreateCodeFixTest(testCode, fixedCode, actionIndex: 0);
        await test.RunAsync();
    }

    // 10. Fix B: Result<T>.Try(() => expr, ex => errorExpr) — custom error handler
    [TestMethod]
    public async Task FixB_PreservesCustomErrorHandler()
    {
        var testCode = @"
using System;
using REslava.Result;
class C
{
    Result<int> GetUser(int id)
    {
        {|RESL1009:try|}
        {
            return id + 1;
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new Error($""Failed: {ex.Message}""));
        }
    }
}";

        var fixedCode = @"
using System;
using REslava.Result;
class C
{
    Result<int> GetUser(int id) =>
        Result<int>.Try(() => id + 1, ex => new Error($""Failed: {ex.Message}""));
}";

        var test = CreateCodeFixTest(testCode, fixedCode, actionIndex: 1);
        await test.RunAsync();
    }

    // 11. Async fix: removes async/await, uses TryAsync
    [TestMethod]
    public async Task FixA_Async_RemovesAsyncAwait_UsesTryAsync()
    {
        var testCode = @"
using System;
using System.Threading.Tasks;
using REslava.Result;
class C
{
    async Task<Result<int>> GetAsync(int id)
    {
        {|RESL1009:try|}
        {
            return await Task.FromResult(id + 1);
        }
        catch (Exception ex)
        {
            return Result<int>.Fail(new ExceptionError(ex));
        }
    }
}";

        var fixedCode = @"
using System;
using System.Threading.Tasks;
using REslava.Result;
class C
{
    Task<Result<int>> GetAsync(int id) =>
        Result<int>.TryAsync(() => Task.FromResult(id + 1));
}";

        var test = CreateCodeFixTest(testCode, fixedCode, actionIndex: 0);
        await test.RunAsync();
    }
}
