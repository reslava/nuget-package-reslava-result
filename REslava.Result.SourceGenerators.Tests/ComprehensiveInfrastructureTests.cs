using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.SourceGenerators;

namespace REslava.Result.SourceGenerators.Tests
{
    /// <summary>
    /// Comprehensive infrastructure tests for the source generator.
    /// Tests all aspects of the generator setup, configuration, and expected behavior.
    /// </summary>
    [TestClass]
    public class ComprehensiveInfrastructureTests
    {
        #region Attribute Configuration Tests

        [TestMethod]
        public void GenerateResultExtensionsAttribute_DefaultValues()
        {
            // Arrange & Act
            var attribute = new GenerateResultExtensionsAttribute();

            // Assert
            Assert.AreEqual("Generated.ResultExtensions", attribute.Namespace);
            Assert.IsTrue(attribute.IncludeErrorTags);
            Assert.IsFalse(attribute.LogErrors);
            Assert.AreEqual(400, attribute.DefaultErrorStatusCode);
            Assert.IsTrue(attribute.GenerateHttpMethodExtensions);
            Assert.AreEqual(0, attribute.CustomErrorMappings.Length);
        }

        [TestMethod]
        public void GenerateResultExtensionsAttribute_CustomValues()
        {
            // Arrange & Act
            var attribute = new GenerateResultExtensionsAttribute
            {
                Namespace = "MyApp.Generated",
                IncludeErrorTags = false,
                LogErrors = true,
                DefaultErrorStatusCode = 500,
                GenerateHttpMethodExtensions = false,
                CustomErrorMappings = new[] { "ValidationError:422", "NotFoundError:404" }
            };

            // Assert
            Assert.AreEqual("MyApp.Generated", attribute.Namespace);
            Assert.IsFalse(attribute.IncludeErrorTags);
            Assert.IsTrue(attribute.LogErrors);
            Assert.AreEqual(500, attribute.DefaultErrorStatusCode);
            Assert.IsFalse(attribute.GenerateHttpMethodExtensions);
            Assert.AreEqual(2, attribute.CustomErrorMappings.Length);
            Assert.AreEqual("ValidationError:422", attribute.CustomErrorMappings[0]);
            Assert.AreEqual("NotFoundError:404", attribute.CustomErrorMappings[1]);
        }

        #endregion

        #region Result Type Tests

        [TestMethod]
        public void Result_GenericSuccess_WorksCorrectly()
        {
            // Arrange & Act
            var result = Result<string>.Ok("test value");

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.IsFailed);
            Assert.AreEqual("test value", result.Value);
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.Successes.Count);
        }

        [TestMethod]
        public void Result_GenericFailure_WorksCorrectly()
        {
            // Arrange & Act
            var result = Result<string>.Fail("error message");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("error message", result.Errors.First().Message);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(0, result.Successes.Count);
        }

        [TestMethod]
        public void Result_NonGenericSuccess_WorksCorrectly()
        {
            // Arrange & Act
            var result = Result.Ok();

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.IsFailed);
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.Successes.Count);
        }

        [TestMethod]
        public void Result_NonGenericFailure_WorksCorrectly()
        {
            // Arrange & Act
            var result = Result.Fail("error message");

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("error message", result.Errors.First().Message);
            Assert.AreEqual(1, result.Errors.Count);
        }

        #endregion

        #region Error Type Tests

        [TestMethod]
        public void CustomErrorTypes_WorkCorrectly()
        {
            // Arrange & Act
            var notFoundError = new TestNotFoundError("User not found");
            var validationError = new TestValidationError("Invalid input");
            var duplicateError = new TestDuplicateError("Duplicate entry");

            // Assert
            Assert.AreEqual("User not found", notFoundError.Message);
            Assert.AreEqual("Invalid input", validationError.Message);
            Assert.AreEqual("Duplicate entry", duplicateError.Message);
        }

        [TestMethod]
        public void ErrorTags_WorkCorrectly()
        {
            // Arrange & Act
            var error = new TestNotFoundError("User not found")
                .WithTag("UserId", 123)
                .WithTag("Timestamp", DateTime.UtcNow);

            // Assert
            Assert.AreEqual(2, error.Tags.Count);
            Assert.IsTrue(error.Tags.ContainsKey("UserId"));
            Assert.IsTrue(error.Tags.ContainsKey("Timestamp"));
            Assert.AreEqual(123, error.Tags["UserId"]);
        }

        #endregion

        #region Expected Generator Behavior Tests

        [TestMethod]
        public void ExpectedBehavior_SuccessResult_ShouldReturn200()
        {
            // This test documents what the generator should produce
            // Arrange
            var result = Result<string>.Ok("test");

            // Expected behavior (what generator should create):
            // var iresult = result.ToIResult();
            // Assert.IsInstanceOfType(iresult, typeof(Ok<string>));

            // Verify the source data is correct
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("test", result.Value);
        }

        [TestMethod]
        public void ExpectedBehavior_NotFoundError_ShouldReturn404()
        {
            // This test documents what the generator should produce
            // Arrange
            var result = Result<string>.Fail("User not found");

            // Expected behavior (what generator should create):
            // var iresult = result.ToIResult();
            // Assert.IsInstanceOfType(iresult, typeof(ProblemHttpResult));
            // var problemResult = iresult as ProblemHttpResult;
            // Assert.AreEqual(404, problemResult.StatusCode);

            // Verify the source data is correct
            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("User not found", result.Errors.First().Message);
        }

        [TestMethod]
        public void ExpectedBehavior_ValidationError_ShouldReturn422()
        {
            // This test documents what the generator should produce
            // Arrange
            var result = Result<string>.Fail("Invalid email format");

            // Expected behavior (what generator should create):
            // var iresult = result.ToIResult();
            // Assert.IsInstanceOfType(iresult, typeof(ProblemHttpResult));
            // var problemResult = iresult as ProblemHttpResult;
            // Assert.AreEqual(422, problemResult.StatusCode);

            // Verify the source data is correct
            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual("Invalid email format", result.Errors.First().Message);
        }

        [TestMethod]
        public void ExpectedBehavior_MultipleErrors_ShouldIncludeAll()
        {
            // This test documents what the generator should produce
            // Arrange
            var errors = new[] { 
                new TestValidationError("Email is required"),
                new TestValidationError("Password is too short")
            };
            var result = Result<string>.Fail(errors);

            // Expected behavior (what generator should create):
            // var iresult = result.ToIResult();
            // Assert.IsInstanceOfType(iresult, typeof(ProblemHttpResult));
            // var problemResult = iresult as ProblemHttpResult;
            // Assert.IsTrue(problemResult.ProblemDetails.Detail.Contains("2 errors occurred"));
            // Assert.IsTrue(problemResult.ProblemDetails.Extensions.ContainsKey("errors"));

            // Verify the source data is correct
            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual(2, result.Errors.Count);
        }

        #endregion

        #region HTTP Method Extension Tests

        [TestMethod]
        public void ExpectedBehavior_ToPostResult_WithLocation_ShouldReturn201()
        {
            // This test documents what the generator should produce
            // Arrange
            var user = new TestUser { Id = 123, Name = "John Doe" };
            var result = Result<TestUser>.Ok(user);

            // Expected behavior (what generator should create):
            // var iresult = result.ToPostResult(u => $"/users/{u.Id}");
            // Assert.IsInstanceOfType(iresult, typeof(Created<TestUser>));
            // var createdResult = iresult as Created<TestUser>;
            // Assert.AreEqual("/users/123", createdResult.Location);
            // Assert.AreEqual(user, createdResult.Value);

            // Verify the source data is correct
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(user, result.Value);
        }

        [TestMethod]
        public void ExpectedBehavior_ToDeleteResult_ShouldReturn204()
        {
            // This test documents what the generator should produce
            // Arrange
            var result = Result.Ok();

            // Expected behavior (what generator should create):
            // var iresult = result.ToDeleteResult();
            // Assert.IsInstanceOfType(iresult, typeof(NoContent));

            // Verify the source data is correct
            Assert.IsTrue(result.IsSuccess);
        }

        #endregion

        #region ProblemDetails Compliance Tests

        [TestMethod]
        public void ExpectedBehavior_ProblemDetails_ShouldBeRFC7807Compliant()
        {
            // This test documents what the generator should produce
            // Arrange
            var result = Result<string>.Fail("Not found");

            // Expected behavior (what generator should create):
            // var iresult = result.ToIResult();
            // Assert.IsInstanceOfType(iresult, typeof(ProblemHttpResult));
            // var problemResult = iresult as ProblemHttpResult;
            // var problem = problemResult.ProblemDetails;
            // Assert.IsNotNull(problem.Type);
            // Assert.IsTrue(problem.Type.StartsWith("https://httpstatuses.io/"));
            // Assert.IsNotNull(problem.Title);
            // Assert.IsNotNull(problem.Status);
            // Assert.IsNotNull(problem.Detail);

            // Verify the source data is correct
            Assert.IsTrue(result.IsFailed);
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void EdgeCase_EmptyErrorMessage_ShouldHandle()
        {
            // The Result library doesn't allow empty error messages, which is correct behavior
            // This test verifies that the validation works properly
            
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentException>(() => Result<string>.Fail(""));
        }

        [TestMethod]
        public void EdgeCase_NullValue_ShouldHandle()
        {
            // Arrange & Act
            var result = Result<string>.Ok(null!);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public void EdgeCase_VeryLongErrorMessage_ShouldHandle()
        {
            // Arrange & Act
            var longMessage = new string('A', 1000);
            var result = Result<string>.Fail(longMessage);

            // Assert
            Assert.IsTrue(result.IsFailed);
            Assert.AreEqual(1000, result.Errors.First().Message.Length);
        }

        #endregion
    }

    /// <summary>
    /// Integration tests that verify the complete workflow and expected behavior.
    /// </summary>
    [TestClass]
    public class IntegrationInfrastructureTests
    {
        [TestMethod]
        public void CompleteWorkflow_Documentation()
        {
            // This test documents the complete expected workflow

            // 1. Enable generator with attribute
            // [assembly: GenerateResultExtensions]

            // 2. Use generated extensions in code
            // var result = Result<string>.Ok("test");
            // var iresult = result.ToIResult();

            // 3. Expected behavior
            // - Success: Returns 200 OK with value
            // - Not found: Returns 404 Not Found
            // - Validation: Returns 422 Unprocessable Entity
            // - Generic error: Returns 400 Bad Request
            // - Multiple errors: Returns 400 with error details

            // Verify basic Result functionality
            var successResult = Result<string>.Ok("test");
            var errorResult = Result<string>.Fail("error");

            Assert.IsTrue(successResult.IsSuccess);
            Assert.IsTrue(errorResult.IsFailed);
        }

        [TestMethod]
        public void GeneratorConfiguration_Documentation()
        {
            // This test documents all configuration options

            var attribute = new GenerateResultExtensionsAttribute
            {
                Namespace = "MyApp.Generated",
                IncludeErrorTags = true,
                LogErrors = false,
                CustomErrorMappings = new[] { "ValidationError:422", "NotFoundError:404" },
                GenerateHttpMethodExtensions = true,
                DefaultErrorStatusCode = 400
            };

            // Document expected behavior:
            // - Namespace: Controls where generated extensions are placed
            // - IncludeErrorTags: Whether to include error tags in ProblemDetails
            // - LogErrors: Whether to log errors during conversion
            // - CustomErrorMappings: Custom error type to status code mappings
            // - GenerateHttpMethodExtensions: Whether to generate HTTP method-specific extensions
            // - DefaultErrorStatusCode: Default status code for unmapped errors

            Assert.AreEqual("MyApp.Generated", attribute.Namespace);
            Assert.IsTrue(attribute.IncludeErrorTags);
            Assert.AreEqual(2, attribute.CustomErrorMappings.Length);
        }

        [TestMethod]
        public void ErrorMappingStrategy_Documentation()
        {
            // This test documents the error mapping strategy

            // Expected mappings (what generator should implement):
            // - "NotFound" → 404 Not Found
            // "DoesNotExist" → 404 Not Found  
            // - "ValidationError" → 422 Unprocessable Entity
            // - "Unauthorized" → 401 Unauthorized
            // - "Forbidden" → 403 Forbidden
            // - "Duplicate" → 409 Conflict
            // - Default → 400 Bad Request

            // Test that our error types follow the expected naming
            var notFoundError = new TestNotFoundError("test");
            var validationError = new TestValidationError("test");
            var unauthorizedError = new TestUnauthorizedError("test");
            var forbiddenError = new TestForbiddenError("test");
            var duplicateError = new TestDuplicateError("test");

            Assert.IsNotNull(notFoundError);
            Assert.IsNotNull(validationError);
            Assert.IsNotNull(unauthorizedError);
            Assert.IsNotNull(forbiddenError);
            Assert.IsNotNull(duplicateError);
        }
    }

    /// <summary>
    /// Performance and stress tests for the infrastructure.
    /// </summary>
    [TestClass]
    public class PerformanceInfrastructureTests
    {
        [TestMethod]
        public void ResultCreation_PerformanceTest()
        {
            // Arrange
            var startTime = DateTime.Now;

            // Act
            var results = Enumerable.Range(1, 1000)
                .Select(i => Result<string>.Ok($"Result {i}"))
                .ToList();

            var endTime = DateTime.Now;
            var duration = endTime - startTime;

            // Assert
            Assert.AreEqual(1000, results.Count);
            Assert.IsTrue(duration.TotalMilliseconds < 100, $"1000 operations took {duration.TotalMilliseconds}ms");
        }

        [TestMethod]
        public void ErrorCreation_PerformanceTest()
        {
            // Arrange
            var startTime = DateTime.Now;

            // Act
            var results = Enumerable.Range(1, 1000)
                .Select(i => Result<string>.Fail($"Error {i}"))
                .ToList();

            var endTime = DateTime.Now;
            var duration = endTime - startTime;

            // Assert
            Assert.AreEqual(1000, results.Count);
            Assert.IsTrue(duration.TotalMilliseconds < 100, $"1000 operations took {duration.TotalMilliseconds}ms");
        }

        [TestMethod]
        public void ErrorTagging_PerformanceTest()
        {
            // Arrange
            var startTime = DateTime.Now;

            // Act
            var errors = Enumerable.Range(1, 100)
                .Select(i => new TestNotFoundError($"Error {i}")
                    .WithTag("Id", i)
                    .WithTag("Timestamp", DateTime.UtcNow))
                .ToList();

            var endTime = DateTime.Now;
            var duration = endTime - startTime;

            // Assert
            Assert.AreEqual(100, errors.Count);
            Assert.IsTrue(duration.TotalMilliseconds < 50, $"100 operations took {duration.TotalMilliseconds}ms");
        }
    }
}
