using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Models;
using System.Collections.Generic;

namespace REslava.Result.SourceGenerators.Tests
{
    [TestClass]
    public class SmartEndpoints_MetadataTests
    {
        [TestMethod]
        public void EndpointMetadata_ShouldContainAllRequiredFields()
        {
            // Arrange
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
}
