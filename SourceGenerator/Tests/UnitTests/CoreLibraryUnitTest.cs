using System;
using REslava.Result.SourceGenerators.Core.CodeGeneration;
using REslava.Result.SourceGenerators.Core.Utilities;
using REslava.Result.SourceGenerators.Generators.ResultToIResult;

namespace REslava.Result.SourceGenerators.Tests.UnitTests;

class CoreLibraryUnitTest
{
    static void Main(string[] args)
    {
        Console.WriteLine("🧪 Core Library Component Tests");
        Console.WriteLine("================================");
        Console.WriteLine();

        int totalTests = 0;
        int passedTests = 0;

        // CodeBuilder Tests
        Console.WriteLine("📝 Testing CodeBuilder...");
        totalTests += 4;
        if (TestCodeBuilder_BasicFunctionality()) passedTests++;
        if (TestCodeBuilder_Indentation()) passedTests++;
        if (TestCodeBuilder_ClassDeclaration()) passedTests++;
        if (TestCodeBuilder_MethodDeclaration()) passedTests++;
        Console.WriteLine($"CodeBuilder: {passedTests}/4 tests passed");
        Console.WriteLine();

        // HttpStatusCodeMapper Tests
        Console.WriteLine("🌐 Testing HttpStatusCodeMapper...");
        int mapperTests = 0;
        int mapperPassed = 0;
        mapperTests += 8;
        if (TestHttpStatusCodeMapper_DefaultConstructor()) mapperPassed++;
        if (TestHttpStatusCodeMapper_CustomDefault()) mapperPassed++;
        if (TestHttpStatusCodeMapper_CustomMapping()) mapperPassed++;
        if (TestHttpStatusCodeMapper_NotFoundConvention()) mapperPassed++;
        if (TestHttpStatusCodeMapper_ValidationConvention()) mapperPassed++;
        if (TestHttpStatusCodeMapper_MultipleMappings()) mapperPassed++;
        if (TestHttpStatusCodeMapper_EmptyErrorName()) mapperPassed++;
        if (TestHttpStatusCodeMapper_NullErrorName()) mapperPassed++;
        Console.WriteLine($"HttpStatusCodeMapper: {mapperPassed}/{mapperTests} tests passed");
        Console.WriteLine();

        // ResultToIResultConfig Tests
        Console.WriteLine("⚙️ Testing ResultToIResultConfig...");
        int configTests = 0;
        int configPassed = 0;
        configTests += 6;
        if (TestResultToIResultConfig_DefaultValues()) configPassed++;
        if (TestResultToIResultConfig_Clone()) configPassed++;
        if (TestResultToIResultConfig_ValidateValid()) configPassed++;
        if (TestResultToIResultConfig_ValidateEmptyNamespace()) configPassed++;
        if (TestResultToIResultConfig_ValidateInvalidStatusCode()) configPassed++;
        if (TestResultToIResultConfig_EdgeCases()) configPassed++;
        Console.WriteLine($"ResultToIResultConfig: {configPassed}/{configTests} tests passed");
        Console.WriteLine();

        totalTests += mapperTests + configTests;
        passedTests += mapperPassed + configPassed;

        // Summary
        Console.WriteLine("🎯 Test Summary");
        Console.WriteLine("================");
        Console.WriteLine($"Total Tests: {totalTests}");
        Console.WriteLine($"Passed: {passedTests}");
        Console.WriteLine($"Failed: {totalTests - passedTests}");
        Console.WriteLine($"Success Rate: {(double)passedTests / totalTests * 100:F1}%");

        if (passedTests == totalTests)
        {
            Console.WriteLine("🎉 All tests passed! Core library components are working correctly!");
        }
        else
        {
            Console.WriteLine("⚠️ Some tests failed. Please review the implementation.");
        }
    }

    #region CodeBuilder Tests

    static bool TestCodeBuilder_BasicFunctionality()
    {
        try
        {
            var builder = new CodeBuilder();
            var result = builder
                .AppendLine("namespace TestNamespace")
                .Indent()
                .AppendLine("public class TestClass")
                .Indent()
                .AppendLine("public void TestMethod() { }")
                .Unindent()
                .Unindent()
                .AppendLine("}")
                .ToString();

            return result.Contains("namespace TestNamespace") &&
                   result.Contains("public class TestClass") &&
                   result.Contains("public void TestMethod() { }") &&
                   result.EndsWith("\r\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCodeBuilder_BasicFunctionality failed: {ex.Message}");
            return false;
        }
    }

    static bool TestCodeBuilder_Indentation()
    {
        try
        {
            var builder = new CodeBuilder();
            var result = builder
                .AppendLine("Level0")
                .Indent()
                .AppendLine("Level1")
                .Indent()
                .AppendLine("Level2")
                .Unindent()
                .AppendLine("Level1Again")
                .Unindent()
                .AppendLine("Level0Again")
                .ToString();

            var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return lines.Length >= 5 &&
                   lines[0].Trim() == "Level0" &&
                   lines[1] == "    Level1" &&
                   lines[2] == "        Level2" &&
                   lines[3] == "    Level1Again" &&
                   lines[4].Trim() == "Level0Again";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCodeBuilder_Indentation failed: {ex.Message}");
            return false;
        }
    }

    static bool TestCodeBuilder_ClassDeclaration()
    {
        try
        {
            var builder = new CodeBuilder();
            var result = builder
                .AppendClassDeclaration("TestClass", "public", "static")
                .AppendLine("public void Method() { }")
                .CloseBrace()
                .ToString();

            return result.Contains("public static class TestClass") &&
                   result.Contains("public void Method() { }");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCodeBuilder_ClassDeclaration failed: {ex.Message}");
            return false;
        }
    }

    static bool TestCodeBuilder_MethodDeclaration()
    {
        try
        {
            var builder = new CodeBuilder();
            var result = builder
                .AppendMethodDeclaration("TestMethod", "string", "T param", "T", "public", "static")
                .AppendLine("return param.ToString();")
                .CloseBrace()
                .ToString();

            return result.Contains("public static string TestMethod<T>(T param)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCodeBuilder_MethodDeclaration failed: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region HttpStatusCodeMapper Tests

    static bool TestHttpStatusCodeMapper_DefaultConstructor()
    {
        try
        {
            var mapper = new HttpStatusCodeMapper();
            var result = mapper.DetermineStatusCode("UnknownError");
            return result == 400;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestHttpStatusCodeMapper_DefaultConstructor failed: {ex.Message}");
            return false;
        }
    }

    static bool TestHttpStatusCodeMapper_CustomDefault()
    {
        try
        {
            var mapper = new HttpStatusCodeMapper(500);
            var result = mapper.DetermineStatusCode("UnknownError");
            return result == 500;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestHttpStatusCodeMapper_CustomDefault failed: {ex.Message}");
            return false;
        }
    }

    static bool TestHttpStatusCodeMapper_CustomMapping()
    {
        try
        {
            var mapper = new HttpStatusCodeMapper();
            mapper.AddMapping("CustomError", 418);
            var result = mapper.DetermineStatusCode("CustomError");
            return result == 418;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestHttpStatusCodeMapper_CustomMapping failed: {ex.Message}");
            return false;
        }
    }

    static bool TestHttpStatusCodeMapper_NotFoundConvention()
    {
        try
        {
            var mapper = new HttpStatusCodeMapper();
            return mapper.DetermineStatusCode("UserNotFoundError") == 404 &&
                   mapper.DetermineStatusCode("ResourceMissingException") == 404 &&
                   mapper.DetermineStatusCode("ItemDoesNotExist") == 404;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestHttpStatusCodeMapper_NotFoundConvention failed: {ex.Message}");
            return false;
        }
    }

    static bool TestHttpStatusCodeMapper_ValidationConvention()
    {
        try
        {
            var mapper = new HttpStatusCodeMapper();
            return mapper.DetermineStatusCode("ValidationError") == 422 &&
                   mapper.DetermineStatusCode("InvalidInputException") == 422 &&
                   mapper.DetermineStatusCode("MalformedDataError") == 422;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestHttpStatusCodeMapper_ValidationConvention failed: {ex.Message}");
            return false;
        }
    }

    static bool TestHttpStatusCodeMapper_MultipleMappings()
    {
        try
        {
            var mapper = new HttpStatusCodeMapper();
            var mappings = new[] { "PaymentError:402", "RateLimitError:429", "TimeoutError:408" };
            mapper.AddMappings(mappings);
            
            return mapper.DetermineStatusCode("PaymentError") == 402 &&
                   mapper.DetermineStatusCode("RateLimitError") == 429 &&
                   mapper.DetermineStatusCode("TimeoutError") == 408;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestHttpStatusCodeMapper_MultipleMappings failed: {ex.Message}");
            return false;
        }
    }

    static bool TestHttpStatusCodeMapper_EmptyErrorName()
    {
        try
        {
            var mapper = new HttpStatusCodeMapper(500);
            var result = mapper.DetermineStatusCode("");
            return result == 500;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestHttpStatusCodeMapper_EmptyErrorName failed: {ex.Message}");
            return false;
        }
    }

    static bool TestHttpStatusCodeMapper_NullErrorName()
    {
        try
        {
            var mapper = new HttpStatusCodeMapper(500);
            var result = mapper.DetermineStatusCode(null!);
            return result == 500;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestHttpStatusCodeMapper_NullErrorName failed: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region ResultToIResultConfig Tests

    static bool TestResultToIResultConfig_DefaultValues()
    {
        try
        {
            var config = new ResultToIResultConfig();
            return config.Namespace == "Generated" &&
                   config.IncludeErrorTags == true &&
                   config.GenerateHttpMethodExtensions == true &&
                   config.DefaultErrorStatusCode == 400 &&
                   config.IncludeDetailedErrors == false &&
                   config.GenerateAsyncMethods == true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestResultToIResultConfig_DefaultValues failed: {ex.Message}");
            return false;
        }
    }

    static bool TestResultToIResultConfig_Clone()
    {
        try
        {
            var original = new ResultToIResultConfig
            {
                Namespace = "Test.Namespace",
                IncludeErrorTags = false,
                CustomErrorMappings = new[] { "TestError:418" },
                DefaultErrorStatusCode = 500
            };

            var clone = (ResultToIResultConfig)original.Clone();

            bool isEqual = original.Namespace == clone.Namespace &&
                          original.IncludeErrorTags == clone.IncludeErrorTags &&
                          original.DefaultErrorStatusCode == clone.DefaultErrorStatusCode &&
                          original.CustomErrorMappings.Length == clone.CustomErrorMappings.Length;

            // Test independence
            clone.Namespace = "Modified.Namespace";
            bool isIndependent = original.Namespace != clone.Namespace;

            return isEqual && isIndependent;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestResultToIResultConfig_Clone failed: {ex.Message}");
            return false;
        }
    }

    static bool TestResultToIResultConfig_ValidateValid()
    {
        try
        {
            var config = new ResultToIResultConfig
            {
                Namespace = "Valid.Namespace",
                DefaultErrorStatusCode = 422
            };
            return config.Validate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestResultToIResultConfig_ValidateValid failed: {ex.Message}");
            return false;
        }
    }

    static bool TestResultToIResultConfig_ValidateEmptyNamespace()
    {
        try
        {
            var config = new ResultToIResultConfig { Namespace = "" };
            return !config.Validate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestResultToIResultConfig_ValidateEmptyNamespace failed: {ex.Message}");
            return false;
        }
    }

    static bool TestResultToIResultConfig_ValidateInvalidStatusCode()
    {
        try
        {
            var config = new ResultToIResultConfig
            {
                Namespace = "Valid.Namespace",
                DefaultErrorStatusCode = 99 // Invalid HTTP status code
            };
            return !config.Validate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestResultToIResultConfig_ValidateInvalidStatusCode failed: {ex.Message}");
            return false;
        }
    }

    static bool TestResultToIResultConfig_EdgeCases()
    {
        try
        {
            // Test null namespace
            var config1 = new ResultToIResultConfig { Namespace = null! };
            bool nullNamespaceFails = !config1.Validate();

            // Test null CustomErrorMappings
            var config2 = new ResultToIResultConfig
            {
                Namespace = "Valid.Namespace",
                CustomErrorMappings = null!
            };
            bool nullMappingsHandled = config2.Validate();

            return nullNamespaceFails && nullMappingsHandled;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestResultToIResultConfig_EdgeCases failed: {ex.Message}");
            return false;
        }
    }

    #endregion
}
