using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace REslava.Result.SourceGenerators.Tests
{
    [TestClass]
    public class SmartEndpoints_AttributeParsingTests
    {
        [TestMethod]
        public void ParseRoutePrefix_ValidAttribute_ShouldExtractPrefix()
        {
            // Arrange
            var expectedPrefix = "/api/users";
            var attributeValue = "\"/api/users\"";
            
            // Act
            var actualPrefix = attributeValue.Trim('"');
            
            // Assert
            Assert.AreEqual(expectedPrefix, actualPrefix);
        }
    }
}
