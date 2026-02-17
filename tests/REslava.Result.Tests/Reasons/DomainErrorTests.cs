using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.Reasons;

[TestClass]
public sealed class DomainErrorTests
{
    #region NotFoundError

    [TestMethod]
    public void NotFoundError_WithMessage_ShouldCreateError()
    {
        var error = new NotFoundError("User not found");

        Assert.AreEqual("User not found", error.Message);
        Assert.AreEqual("NotFound", error.Tags["ErrorType"]);
        Assert.AreEqual(404, error.Tags["HttpStatusCode"]);
    }

    [TestMethod]
    public void NotFoundError_WithEntityAndId_ShouldCreateDescriptiveMessage()
    {
        var error = new NotFoundError("User", 42);

        Assert.AreEqual("User with id '42' was not found", error.Message);
        Assert.AreEqual("User", error.Tags["EntityName"]);
        Assert.AreEqual("42", error.Tags["EntityId"]);
        Assert.AreEqual(404, error.Tags["HttpStatusCode"]);
    }

    [TestMethod]
    public void NotFoundError_WithTag_ShouldReturnNotFoundError()
    {
        var error = new NotFoundError("Not found")
            .WithTag("Scope", "Database");

        Assert.IsInstanceOfType<NotFoundError>(error);
        Assert.AreEqual("Database", error.Tags["Scope"]);
    }

    [TestMethod]
    public void NotFoundError_InResult_ShouldWork()
    {
        var result = Result<string>.Fail(new NotFoundError("User", 1));

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<NotFoundError>(result.Errors[0]);
    }

    #endregion

    #region ValidationError

    [TestMethod]
    public void ValidationError_WithMessage_ShouldCreateError()
    {
        var error = new ValidationError("Name is required");

        Assert.AreEqual("Name is required", error.Message);
        Assert.AreEqual("Validation", error.Tags["ErrorType"]);
        Assert.AreEqual(422, error.Tags["HttpStatusCode"]);
        Assert.IsNull(error.FieldName);
    }

    [TestMethod]
    public void ValidationError_WithFieldAndMessage_ShouldSetFieldName()
    {
        var error = new ValidationError("Email", "Invalid email format");

        Assert.AreEqual("Invalid email format", error.Message);
        Assert.AreEqual("Email", error.FieldName);
        Assert.AreEqual("Email", error.Tags["FieldName"]);
    }

    [TestMethod]
    public void ValidationError_WithTag_ShouldReturnValidationError()
    {
        var error = new ValidationError("Email", "Invalid format")
            .WithTag("ProvidedValue", "not-an-email");

        Assert.IsInstanceOfType<ValidationError>(error);
        Assert.AreEqual("Email", error.FieldName);
        Assert.AreEqual("not-an-email", error.Tags["ProvidedValue"]);
    }

    [TestMethod]
    public void ValidationError_WithMessage_ShouldPreserveFieldName()
    {
        var error = new ValidationError("Email", "Invalid format")
            .WithMessage("Updated message");

        Assert.IsInstanceOfType<ValidationError>(error);
        Assert.AreEqual("Updated message", error.Message);
        Assert.AreEqual("Email", error.FieldName);
    }

    [TestMethod]
    public void ValidationError_InResult_ShouldWork()
    {
        var result = Result<string>.Fail(new ValidationError("Email", "Invalid"));

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ValidationError>(result.Errors[0]);
    }

    #endregion

    #region ConflictError

    [TestMethod]
    public void ConflictError_WithMessage_ShouldCreateError()
    {
        var error = new ConflictError("Duplicate entry");

        Assert.AreEqual("Duplicate entry", error.Message);
        Assert.AreEqual("Conflict", error.Tags["ErrorType"]);
        Assert.AreEqual(409, error.Tags["HttpStatusCode"]);
    }

    [TestMethod]
    public void ConflictError_WithEntityFieldValue_ShouldCreateDescriptiveMessage()
    {
        var error = new ConflictError("User", "email", "test@test.com");

        Assert.AreEqual("User with email 'test@test.com' already exists", error.Message);
        Assert.AreEqual("User", error.Tags["EntityName"]);
        Assert.AreEqual("email", error.Tags["ConflictField"]);
        Assert.AreEqual("test@test.com", error.Tags["ConflictValue"]);
    }

    [TestMethod]
    public void ConflictError_WithTag_ShouldReturnConflictError()
    {
        var error = new ConflictError("Duplicate")
            .WithTag("Table", "Users");

        Assert.IsInstanceOfType<ConflictError>(error);
    }

    [TestMethod]
    public void ConflictError_InResult_ShouldWork()
    {
        var result = Result<string>.Fail(new ConflictError("User", "email", "a@b.com"));

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ConflictError>(result.Errors[0]);
    }

    #endregion

    #region UnauthorizedError

    [TestMethod]
    public void UnauthorizedError_Default_ShouldHaveDefaultMessage()
    {
        var error = new UnauthorizedError();

        Assert.AreEqual("Authentication required", error.Message);
        Assert.AreEqual("Unauthorized", error.Tags["ErrorType"]);
        Assert.AreEqual(401, error.Tags["HttpStatusCode"]);
    }

    [TestMethod]
    public void UnauthorizedError_WithMessage_ShouldUseCustomMessage()
    {
        var error = new UnauthorizedError("Token has expired");

        Assert.AreEqual("Token has expired", error.Message);
        Assert.AreEqual(401, error.Tags["HttpStatusCode"]);
    }

    [TestMethod]
    public void UnauthorizedError_WithTag_ShouldReturnUnauthorizedError()
    {
        var error = new UnauthorizedError()
            .WithTag("Scheme", "Bearer");

        Assert.IsInstanceOfType<UnauthorizedError>(error);
        Assert.AreEqual("Bearer", error.Tags["Scheme"]);
    }

    [TestMethod]
    public void UnauthorizedError_InResult_ShouldWork()
    {
        var result = Result<string>.Fail(new UnauthorizedError());

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<UnauthorizedError>(result.Errors[0]);
    }

    #endregion

    #region ForbiddenError

    [TestMethod]
    public void ForbiddenError_Default_ShouldHaveDefaultMessage()
    {
        var error = new ForbiddenError();

        Assert.AreEqual("Access denied", error.Message);
        Assert.AreEqual("Forbidden", error.Tags["ErrorType"]);
        Assert.AreEqual(403, error.Tags["HttpStatusCode"]);
    }

    [TestMethod]
    public void ForbiddenError_WithMessage_ShouldUseCustomMessage()
    {
        var error = new ForbiddenError("Admin role required");

        Assert.AreEqual("Admin role required", error.Message);
        Assert.AreEqual(403, error.Tags["HttpStatusCode"]);
    }

    [TestMethod]
    public void ForbiddenError_WithActionAndResource_ShouldCreateDescriptiveMessage()
    {
        var error = new ForbiddenError("Delete", "Order");

        Assert.AreEqual("Access denied: insufficient permissions to Delete Order", error.Message);
        Assert.AreEqual("Delete", error.Tags["Action"]);
        Assert.AreEqual("Order", error.Tags["Resource"]);
    }

    [TestMethod]
    public void ForbiddenError_WithTag_ShouldReturnForbiddenError()
    {
        var error = new ForbiddenError()
            .WithTag("RequiredRole", "Admin");

        Assert.IsInstanceOfType<ForbiddenError>(error);
        Assert.AreEqual("Admin", error.Tags["RequiredRole"]);
    }

    [TestMethod]
    public void ForbiddenError_InResult_ShouldWork()
    {
        var result = Result<string>.Fail(new ForbiddenError("Delete", "Order"));

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<ForbiddenError>(result.Errors[0]);
    }

    #endregion

    #region Pattern Matching

    [TestMethod]
    public void DomainErrors_ShouldSupportPatternMatching()
    {
        var result = Result<string>.Fail(new NotFoundError("User", 1));

        var statusCode = result.Errors[0] switch
        {
            NotFoundError => 404,
            ValidationError => 422,
            ConflictError => 409,
            UnauthorizedError => 401,
            ForbiddenError => 403,
            _ => 500
        };

        Assert.AreEqual(404, statusCode);
    }

    [TestMethod]
    public void DomainErrors_AllImplementIError()
    {
        IError[] errors =
        [
            new NotFoundError("Not found"),
            new ValidationError("Invalid"),
            new ConflictError("Conflict"),
            new UnauthorizedError(),
            new ForbiddenError()
        ];

        Assert.AreEqual(5, errors.Length);
        foreach (var error in errors)
        {
            Assert.IsFalse(string.IsNullOrEmpty(error.Message));
            Assert.IsTrue(error.Tags.ContainsKey("ErrorType"));
            Assert.IsTrue(error.Tags.ContainsKey("HttpStatusCode"));
        }
    }

    [TestMethod]
    public void DomainErrors_HttpStatusCodeTag_ShouldBeCorrect()
    {
        Assert.AreEqual(404, new NotFoundError("x").Tags["HttpStatusCode"]);
        Assert.AreEqual(422, new ValidationError("x").Tags["HttpStatusCode"]);
        Assert.AreEqual(409, new ConflictError("x").Tags["HttpStatusCode"]);
        Assert.AreEqual(401, new UnauthorizedError().Tags["HttpStatusCode"]);
        Assert.AreEqual(403, new ForbiddenError().Tags["HttpStatusCode"]);
    }

    #endregion

    #region Immutability

    [TestMethod]
    public void DomainErrors_ShouldBeImmutable()
    {
        var original = new NotFoundError("User", 42);
        var modified = original.WithTag("Extra", "value");

        Assert.AreNotSame(original, modified);
        Assert.IsFalse(original.Tags.ContainsKey("Extra"));
        Assert.IsTrue(modified.Tags.ContainsKey("Extra"));
    }

    [TestMethod]
    public void ValidationError_WithMessage_ShouldCreateNewInstance()
    {
        var original = new ValidationError("Email", "Bad format");
        var modified = original.WithMessage("Updated");

        Assert.AreNotSame(original, modified);
        Assert.AreEqual("Bad format", original.Message);
        Assert.AreEqual("Updated", modified.Message);
    }

    #endregion
}
