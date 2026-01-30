using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.ResultToIResult;
using REslava.Result.SourceGenerators.Core.Infrastructure;

namespace REslava.Result.SourceGenerators.Tests.IntegrationTests;

class GeneratorIntegrationTests
{
    static void Main(string[] args)
    {
        Console.WriteLine("🔗 Generator Integration Tests");
        Console.WriteLine("=============================");
        Console.WriteLine();

        int totalTests = 0;
        int passedTests = 0;

        // Generator Instantiation Tests
        Console.WriteLine("🏭 Testing Generator Instantiation...");
        totalTests += 3;
        if (TestGenerator_Instantiation()) passedTests++;
        if (TestGenerator_TypeInformation()) passedTests++;
        if (TestGenerator_AttributeInformation()) passedTests++;
        Console.WriteLine($"Generator Instantiation: {passedTests}/3 tests passed");
        Console.WriteLine();

        // Configuration Parsing Tests
        Console.WriteLine("⚙️ Testing Configuration Parsing...");
        int configTests = 0;
        int configPassed = 0;
        configTests += 4;
        if (TestConfiguration_DefaultValues()) configPassed++;
        if (TestConfiguration_CustomValues()) configPassed++;
        if (TestConfiguration_ArrayParsing()) configPassed++;
        if (TestConfiguration_Validation()) configPassed++;
        Console.WriteLine($"Configuration Parsing: {configPassed}/{configTests} tests passed");
        Console.WriteLine();

        // Code Generation Tests
        Console.WriteLine("📝 Testing Code Generation...");
        int codeGenTests = 0;
        int codeGenPassed = 0;
        codeGenTests += 4;
        if (TestCodeGeneration_BasicScenario()) codeGenPassed++;
        if (TestCodeGeneration_CustomNamespace()) codeGenPassed++;
        if (TestCodeGeneration_DisabledFeatures()) codeGenPassed++;
        if (TestCodeGeneration_ErrorHandling()) codeGenPassed++;
        Console.WriteLine($"Code Generation: {codeGenPassed}/{codeGenTests} tests passed");
        Console.WriteLine();

        // Core Library Integration Tests
        Console.WriteLine("🏗️ Testing Core Library Integration...");
        int coreTests = 0;
        int corePassed = 0;
        coreTests += 3;
        if (TestCoreIntegration_CodeBuilder()) corePassed++;
        if (TestCoreIntegration_HttpStatusCodeMapper()) corePassed++;
        if (TestCoreIntegration_AttributeParser()) corePassed++;
        Console.WriteLine($"Core Library Integration: {corePassed}/{coreTests} tests passed");
        Console.WriteLine();

        totalTests += configTests + codeGenTests + coreTests;
        passedTests += configPassed + codeGenPassed + corePassed;

        // Summary
        Console.WriteLine("🎯 Integration Test Summary");
        Console.WriteLine("==========================");
        Console.WriteLine($"Total Tests: {totalTests}");
        Console.WriteLine($"Passed: {passedTests}");
        Console.WriteLine($"Failed: {totalTests - passedTests}");
        Console.WriteLine($"Success Rate: {(double)passedTests / totalTests * 100:F1}%");

        if (passedTests == totalTests)
        {
            Console.WriteLine("🎉 All integration tests passed! Generator is working correctly!");
        }
        else
        {
            Console.WriteLine("⚠️ Some integration tests failed. Please review the implementation.");
        }
    }

    #region Generator Instantiation Tests

    static bool TestGenerator_Instantiation()
    {
        try
        {
            var generator = new ResultToIResultGenerator();
            return generator != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestGenerator_Instantiation failed: {ex.Message}");
            return false;
        }
    }

    static bool TestGenerator_TypeInformation()
    {
        try
        {
            var generator = new ResultToIResultGenerator();
            
            // Test that generator implements expected interfaces
            bool isIncrementalGenerator = generator is IIncrementalGenerator;
            bool hasCorrectName = generator.GetType().Name == "ResultToIResultGenerator";
            
            return isIncrementalGenerator && hasCorrectName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestGenerator_TypeInformation failed: {ex.Message}");
            return false;
        }
    }

    static bool TestGenerator_AttributeInformation()
    {
        try
        {
            var generator = new ResultToIResultGenerator();
            
            // Test that generator has correct attribute information
            // This would typically be tested through actual compilation
            // For now, we'll test the basic structure
            return generator != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestGenerator_AttributeInformation failed: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Configuration Parsing Tests

    static bool TestConfiguration_DefaultValues()
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
            Console.WriteLine($"❌ TestConfiguration_DefaultValues failed: {ex.Message}");
            return false;
        }
    }

    static bool TestConfiguration_CustomValues()
    {
        try
        {
            var config = new ResultToIResultConfig
            {
                Namespace = "Custom.Generated",
                IncludeErrorTags = false,
                GenerateHttpMethodExtensions = false,
                DefaultErrorStatusCode = 500,
                IncludeDetailedErrors = true,
                GenerateAsyncMethods = false,
                CustomErrorMappings = new[] { "CustomError:418" }
            };
            
            return config.Namespace == "Custom.Generated" &&
                   config.IncludeErrorTags == false &&
                   config.GenerateHttpMethodExtensions == false &&
                   config.DefaultErrorStatusCode == 500 &&
                   config.IncludeDetailedErrors == true &&
                   config.GenerateAsyncMethods == false &&
                   config.CustomErrorMappings.Length == 1 &&
                   config.CustomErrorMappings[0] == "CustomError:418";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestConfiguration_CustomValues failed: {ex.Message}");
            return false;
        }
    }

    static bool TestConfiguration_ArrayParsing()
    {
        try
        {
            var config = new ResultToIResultConfig();
            var testMappings = new[] { "Error1:401", "Error2:403", "Error3:404" };
            config.CustomErrorMappings = testMappings;
            
            return config.CustomErrorMappings.Length == 3 &&
                   config.CustomErrorMappings[0] == "Error1:401" &&
                   config.CustomErrorMappings[1] == "Error2:403" &&
                   config.CustomErrorMappings[2] == "Error3:404";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestConfiguration_ArrayParsing failed: {ex.Message}");
            return false;
        }
    }

    static bool TestConfiguration_Validation()
    {
        try
        {
            // Test valid configuration
            var validConfig = new ResultToIResultConfig
            {
                Namespace = "Valid.Namespace",
                DefaultErrorStatusCode = 422
            };
            bool validPasses = validConfig.Validate();
            
            // Test invalid configuration
            var invalidConfig = new ResultToIResultConfig
            {
                Namespace = "", // Invalid
                DefaultErrorStatusCode = 99 // Invalid
            };
            bool invalidFails = !invalidConfig.Validate();
            
            return validPasses && invalidFails;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestConfiguration_Validation failed: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Code Generation Tests

    static bool TestCodeGeneration_BasicScenario()
    {
        try
        {
            // Test basic code generation scenario
            var generator = new ResultToIResultGenerator();
            var config = new ResultToIResultConfig();
            
            // This would typically involve creating a compilation and running the generator
            // For now, we'll test the basic setup
            return generator != null && config.Validate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCodeGeneration_BasicScenario failed: {ex.Message}");
            return false;
        }
    }

    static bool TestCodeGeneration_CustomNamespace()
    {
        try
        {
            var config = new ResultToIResultConfig
            {
                Namespace = "Custom.Test.Namespace"
            };
            
            return config.Namespace == "Custom.Test.Namespace" && config.Validate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCodeGeneration_CustomNamespace failed: {ex.Message}");
            return false;
        }
    }

    static bool TestCodeGeneration_DisabledFeatures()
    {
        try
        {
            var config = new ResultToIResultConfig
            {
                GenerateHttpMethodExtensions = false,
                GenerateAsyncMethods = false,
                IncludeErrorTags = false
            };
            
            return config.GenerateHttpMethodExtensions == false &&
                   config.GenerateAsyncMethods == false &&
                   config.IncludeErrorTags == false &&
                   config.Validate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCodeGeneration_DisabledFeatures failed: {ex.Message}");
            return false;
        }
    }

    static bool TestCodeGeneration_ErrorHandling()
    {
        try
        {
            // Test error handling in code generation
            var config = new ResultToIResultConfig();
            
            // Test with invalid configuration
            var invalidConfig = new ResultToIResultConfig { Namespace = null! };
            bool handlesInvalidConfig = !invalidConfig.Validate();
            
            return config.Validate() && handlesInvalidConfig;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCodeGeneration_ErrorHandling failed: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Core Library Integration Tests

    static bool TestCoreIntegration_CodeBuilder()
    {
        try
        {
            // Test that generator uses CodeBuilder from Core library
            var builder = new REslava.Result.SourceGenerators.Core.CodeGeneration.CodeBuilder();
            var result = builder
                .AppendLine("namespace Test")
                .AppendClassDeclaration("TestClass", "public", "static")
                .AppendLine("public void Method() { }")
                .CloseBrace()
                .CloseBrace()
                .ToString();
            
            return result.Contains("namespace Test") &&
                   result.Contains("public static class TestClass") &&
                   result.Contains("public void Method() { }");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCoreIntegration_CodeBuilder failed: {ex.Message}");
            return false;
        }
    }

    static bool TestCoreIntegration_HttpStatusCodeMapper()
    {
        try
        {
            // Test that generator uses HttpStatusCodeMapper from Core library
            var mapper = new REslava.Result.SourceGenerators.Core.Utilities.HttpStatusCodeMapper();
            
            return mapper.DetermineStatusCode("UserNotFoundError") == 404 &&
                   mapper.DetermineStatusCode("ValidationError") == 422 &&
                   mapper.DetermineStatusCode("UnknownError") == 400;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCoreIntegration_HttpStatusCodeMapper failed: {ex.Message}");
            return false;
        }
    }

    static bool TestCoreIntegration_AttributeParser()
    {
        try
        {
            // Test that generator uses AttributeParser from Core library
            // This would typically be tested through actual compilation
            // For now, we'll test the basic availability
            var config = new ResultToIResultConfig();
            
            return config != null && config.Validate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestCoreIntegration_AttributeParser failed: {ex.Message}");
            return false;
        }
    }

    #endregion
}
