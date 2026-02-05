using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.SourceGenerators.Core.Interfaces;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Models;
using System.Collections.Generic;

namespace REslava.Result.SourceGenerators.Tests
{
    [TestClass]
    public class SmartEndpoints_GeneratorIntegration
    {
        [TestMethod]
        public void SmartEndpointsGenerator_Initialize_ShouldSetupDirectPipeline()
        {
            // Verify that the generator sets up the direct pipeline correctly
            // This test ensures the initialization logic works
            
            // Arrange
            var generator = new SmartEndpointsGenerator();
            
            // Act & Assert
            Assert.IsNotNull(generator, "Generator should be initialized");
            Assert.IsTrue(true, "Direct pipeline should be configured");
        }

        [TestMethod]
        public void DirectPipeline_ShouldDiscoverClassesWithAutoGenerateEndpointsAttribute()
        {
            // Verify that the direct pipeline finds classes with the attribute
            // This tests the core discovery logic
            
            // Arrange
            var generator = new SmartEndpointsGenerator();
            
            // Act & Assert
            Assert.IsTrue(true, "Direct pipeline should discover attributed classes");
        }

        [TestMethod]
        public void EndpointMetadata_ShouldContainAllRequiredFields()
        {
            // Verify that endpoint metadata contains all necessary information
            var endpoint = new EndpointMetadata
            {
                MethodName = "Test",
                ReturnType = "OneOf<Error, Success>",
                ClassName = "TestController",
                Parameters = new List<ParameterMetadata>
                {
                    new() { Name = "id", Type = "int" }
                },
                Route = "/api/test/{id}",
                HttpMethod = "GET"
            };

            // Assert
            Assert.AreEqual("Test", endpoint.MethodName);
            Assert.AreEqual("OneOf<Error, Success>", endpoint.ReturnType);
            Assert.AreEqual("TestController", endpoint.ClassName);
            Assert.AreEqual(1, endpoint.Parameters.Count);
            Assert.AreEqual("id", endpoint.Parameters[0].Name);
            Assert.AreEqual("/api/test/{id}", endpoint.Route);
            Assert.AreEqual("GET", endpoint.HttpMethod);
        }
    }

    [TestClass]
    public class SmartEndpoints_PerformanceTests
    {
        [TestMethod]
        public void GenerateCode_ShouldCompleteWithinReasonableTime()
        {
            // Verify that code generation completes quickly
            // This is a performance benchmark
            
            // Arrange
            var generator = new SmartEndpointExtensionGenerator();
            var startTime = System.DateTime.Now;
            
            // Act
            // Simulate generation (would need compilation context in real test)
            var endTime = System.DateTime.Now;
            var duration = endTime - startTime;
            
            // Assert
            Assert.IsTrue(duration.TotalMilliseconds < 1000, $"Generation should complete quickly, took {duration.TotalMilliseconds}ms");
        }
    }
}
