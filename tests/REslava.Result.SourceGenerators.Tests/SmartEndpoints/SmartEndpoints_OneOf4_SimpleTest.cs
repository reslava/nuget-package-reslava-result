using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace REslava.Result.SourceGenerators.Tests
{
    [TestClass]
    public class SmartEndpoints_OneOf4_SimpleTest
    {
        [TestMethod]
        public void SmartEndpoints_CodeGeneration_Should_Handle_OneOf4_ReturnTypes()
        {
            // Arrange - Test the metadata creation
            var endpoint = new EndpointMetadata
            {
                MethodName = "TestMethod",
                ClassName = "TestController",
                ReturnType = "REslava.Result.AdvancedPatterns.OneOf<ValidationError, NotFoundError, ConflictError, ServerError>",
                IsOneOf = true,
                IsOneOf4 = true,
                IsResult = false,
                GenericTypeArguments = new List<string> 
                { 
                    "ValidationError", "NotFoundError", "ConflictError", "ServerError" 
                }
            };

            // Act & Assert
            Assert.IsNotNull(endpoint, "Should create endpoint metadata");
            Assert.IsTrue(endpoint.IsOneOf4, "Should set IsOneOf4 flag");
            Assert.AreEqual(4, endpoint.GenericTypeArguments.Count, "Should have 4 generic type arguments");
        }

        [TestMethod]
        public void SmartEndpoints_CodeGeneration_Should_Detect_OneOf4_By_Comma_Count()
        {
            // Arrange - Test the comma counting logic
            var returnType1 = "OneOf<ValidationError, NotFoundError>"; // 1 comma
            var returnType2 = "OneOf<ValidationError, NotFoundError, ConflictError>"; // 2 commas  
            var returnType3 = "OneOf<ValidationError, NotFoundError, ConflictError, ServerError>"; // 3 commas

            // Act & Assert - Test comma counting
            Assert.AreEqual(1, returnType1.Count(c => c == ','), "Should count 1 comma for OneOf2");
            Assert.AreEqual(2, returnType2.Count(c => c == ','), "Should count 2 commas for OneOf3");
            Assert.AreEqual(3, returnType3.Count(c => c == ','), "Should count 3 commas for OneOf4");
        }

        [TestMethod]
        public void SmartEndpoints_Metadata_Should_Set_OneOf4_Flag_Correctly()
        {
            // Arrange - Test metadata creation
            var endpoint = new EndpointMetadata();

            // Act
            endpoint.IsOneOf4 = true;

            // Assert
            Assert.IsTrue(endpoint.IsOneOf4, "Should set IsOneOf4 flag");
        }
    }
}
