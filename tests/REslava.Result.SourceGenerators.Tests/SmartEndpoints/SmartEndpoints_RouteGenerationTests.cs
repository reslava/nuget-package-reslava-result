using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.SourceGenerators.SmartEndpoints.Models;
using System.Collections.Generic;

namespace REslava.Result.SourceGenerators.Tests
{
    [TestClass]
    public class SmartEndpoints_RouteGenerationTests
    {
        [TestMethod]
        public void InferRoute_GetUser_WithIdParameter_ShouldIncludeIdInRoute()
        {
            // Arrange
            var parameters = new List<ParameterMetadata>
            {
                new() { Name = "id", Type = "int" }
            };

            // Act
            var route = SmartEndpointsOrchestrator.InferRouteFromMethodName("GetUser", parameters);

            // Assert
            Assert.AreEqual("/User/{id}", route);
        }

        [TestMethod]
        public void InferRoute_CreateUser_NoIdParameter_ShouldNotIncludeId()
        {
            // Arrange
            var parameters = new List<ParameterMetadata>
            {
                new() { Name = "request", Type = "CreateUserRequest" }
            };

            // Act
            var route = SmartEndpointsOrchestrator.InferRouteFromMethodName("CreateUser", parameters);

            // Assert
            Assert.AreEqual("", route);
        }
    }
}
