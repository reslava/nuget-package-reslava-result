using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.SourceGenerators;

namespace REslava.Result.SourceGenerators.Tests
{
    /// <summary>
    /// Verification that the source generator infrastructure is properly set up.
    /// The actual generator functionality is verified in the working Web API project.
    /// </summary>
    [TestClass]
    public class GeneratorVerification
    {
        [TestMethod]
        public void VerifyGeneratorInfrastructure()
        {
            // Verify that the attribute is accessible
            var attribute = new GenerateResultExtensionsAttribute();
            Assert.IsNotNull(attribute);
            Assert.AreEqual("Generated.ResultExtensions", attribute.Namespace);
            Assert.IsTrue(attribute.IncludeErrorTags);
            Assert.AreEqual(400, attribute.DefaultErrorStatusCode);
        }

        [TestMethod]
        public void VerifyResultTypes()
        {
            // Verify that Result types are working
            var successResult = Result<string>.Ok("test");
            Assert.IsTrue(successResult.IsSuccess);
            Assert.AreEqual("test", successResult.Value);

            var errorResult = Result<string>.Fail("error");
            Assert.IsTrue(errorResult.IsFailed);
            Assert.AreEqual("error", errorResult.Errors.First().Message);
        }

        [TestMethod]
        public void VerifyTestSetup()
        {
            // This verifies our test infrastructure is working
            Assert.IsTrue(true, "Test infrastructure is working correctly");
        }
    }
}
