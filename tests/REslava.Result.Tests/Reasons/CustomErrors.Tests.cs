// using System.Collections.Immutable;

// namespace REslava.Result.Reasons.Tests;

// /// <summary>
// /// Comprehensive tests for custom error implementations
// /// </summary>
// [TestClass]
// public sealed class CustomErrorsTests
// {
//     #region Custom Error Implementations

//     // Simple custom error - inherits from Error
//     private class ValidationError : Error
//     {
//         public ValidationError(string field, string message)
//             : base($"{field}: {message}")
//         {
//             var temp = WithTags("Field", field)
//                 .WithTags("ErrorType", "Validation")
//                 .WithTags("Severity", "Warning");
            
//             // Copy back (ugly but shows the pattern)
//             // In real usage, you'd chain the constructor call
//         }

//         private ValidationError(string message, ImmutableDictionary<string, object> tags)
//             : base(message, tags) { }
//     }

//     // Advanced custom error - direct CRTP inheritance
//     private class DatabaseError : Reason<DatabaseError>, IError
//     {
//         public DatabaseError(string message) : base(message) { }

//         private DatabaseError(string message, ImmutableDictionary<string, object> tags)
//             : base(message, tags) { }

//         protected override DatabaseError CreateNew(string message, ImmutableDictionary<string, object> tags)
//         {
//             return new DatabaseError(message, tags);
//         }

//         // Custom fluent method specific to DatabaseError
//         public DatabaseError WithQuery(string query)
//         {
//             return WithTags("Query", query);
//         }

//         public DatabaseError WithRetryCount(int count)
//         {
//             return WithTags("RetryCount", count);
//         }
//     }

//     // NotFoundError with specific factory methods
//     private class NotFoundError : Reason<NotFoundError>, IError
//     {
//         private NotFoundError(string message) : base(message) { }

//         private NotFoundError(string message, ImmutableDictionary<string, object> tags)
//             : base(message, tags) { }

//         protected override NotFoundError CreateNew(string message, ImmutableDictionary<string, object> tags)
//         {
//             return new NotFoundError(message, tags);
//         }

//         // Factory methods
//         public static NotFoundError Entity(string entityType, string id)
//         {
//             return new NotFoundError($"{entityType} with id '{id}' not found")
//                 .WithTags("EntityType", entityType)
//                 .WithTags("EntityId", id)
//                 .WithTags("StatusCode", 404);
//         }

//         public static NotFoundError User(string userId)
//         {
//             return Entity("User", userId);
//         }

//         public static NotFoundError Order(string orderId)
//         {
//             return Entity("Order", orderId);
//         }
//     }

//     // Authorization error with permission tracking
//     private class AuthorizationError : Reason<AuthorizationError>, IError
//     {
//         private AuthorizationError(string message) : base(message) { }

//         private AuthorizationError(string message, ImmutableDictionary<string, object> tags)
//             : base(message, tags) { }

//         protected override AuthorizationError CreateNew(string message, ImmutableDictionary<string, object> tags)
//         {
//             return new AuthorizationError(message, tags);
//         }

//         public static AuthorizationError MissingPermission(string userId, string permission)
//         {
//             return new AuthorizationError($"User '{userId}' lacks permission '{permission}'")
//                 .WithTags("UserId", userId)
//                 .WithTags("RequiredPermission", permission)
//                 .WithTags("StatusCode", 403);
//         }

//         public static AuthorizationError Forbidden(string resource)
//         {
//             return new AuthorizationError($"Access to '{resource}' is forbidden")
//                 .WithTags("Resource", resource)
//                 .WithTags("StatusCode", 403);
//         }
//     }

//     #endregion

//     #region Validation Error Tests

//     [TestMethod]
//     public void ValidationError_Constructor_SetsCorrectMessage()
//     {
//         // Act
//         var error = new ValidationError("Email", "Invalid format");

//         // Assert
//         Assert.AreEqual("Email: Invalid format", error.Message);
//     }

//     [TestMethod]
//     public void ValidationError_ImplementsIError()
//     {
//         // Arrange
//         var error = new ValidationError("Field", "Message");

//         // Assert
//         Assert.IsInstanceOfType<IError>(error);
//     }

//     [TestMethod]
//     public void ValidationError_InheritsFromError()
//     {
//         // Arrange
//         var error = new ValidationError("Field", "Message");

//         // Assert
//         Assert.IsInstanceOfType<Error>(error);
//     }

//     [TestMethod]
//     public void ValidationError_UsedInResult_Works()
//     {
//         // Arrange
//         var error = new ValidationError("Email", "Invalid email format");

//         // Act
//         var result = Result<string>.Fail(error);

//         // Assert
//         Assert.IsTrue(result.IsFailed);
//         Assert.AreEqual("Email: Invalid email format", result.Errors[0].Message);
//     }

//     #endregion

//     #region Database Error Tests

//     [TestMethod]
//     public void DatabaseError_Constructor_CreatesError()
//     {
//         // Act
//         var error = new DatabaseError("Connection failed");

//         // Assert
//         Assert.AreEqual("Connection failed", error.Message);
//         Assert.IsTrue(error.Tags.IsEmpty);
//     }

//     [TestMethod]
//     public void DatabaseError_WithQuery_AddsQueryTag()
//     {
//         // Act
//         var error = new DatabaseError("Query failed")
//             .WithQuery("SELECT * FROM Users");

//         // Assert
//         Assert.IsTrue(error.Tags.ContainsKey("Query"));
//         Assert.AreEqual("SELECT * FROM Users", error.Tags["Query"]);
//     }

//     [TestMethod]
//     public void DatabaseError_WithRetryCount_AddsRetryCountTag()
//     {
//         // Act
//         var error = new DatabaseError("Connection timeout")
//             .WithRetryCount(3);

//         // Assert
//         Assert.IsTrue(error.Tags.ContainsKey("RetryCount"));
//         Assert.AreEqual(3, error.Tags["RetryCount"]);
//     }

//     [TestMethod]
//     public void DatabaseError_FluentChaining_Works()
//     {
//         // Act
//         var error = new DatabaseError("Database operation failed")
//             .WithQuery("INSERT INTO Orders")
//             .WithRetryCount(3)
//             .WithTags("Server", "localhost")
//             .WithTags("Database", "Production");

//         // Assert
//         Assert.AreEqual("Database operation failed", error.Message);
//         Assert.AreEqual(4, error.Tags.Count);
//         Assert.AreEqual("INSERT INTO Orders", error.Tags["Query"]);
//         Assert.AreEqual(3, error.Tags["RetryCount"]);
//         Assert.AreEqual("localhost", error.Tags["Server"]);
//         Assert.AreEqual("Production", error.Tags["Database"]);
//     }

//     [TestMethod]
//     public void DatabaseError_CustomFluentMethods_ReturnDatabaseErrorType()
//     {
//         // Arrange
//         var error = new DatabaseError("Test");

//         // Act
//         DatabaseError e1 = error.WithQuery("SELECT 1");
//         DatabaseError e2 = e1.WithRetryCount(5);
//         DatabaseError e3 = e2.WithMessage("Updated");

//         // Assert - All return DatabaseError
//         Assert.IsInstanceOfType<DatabaseError>(e1);
//         Assert.IsInstanceOfType<DatabaseError>(e2);
//         Assert.IsInstanceOfType<DatabaseError>(e3);
//     }

//     [TestMethod]
//     public void DatabaseError_ImplementsIError()
//     {
//         // Arrange
//         var error = new DatabaseError("Test");

//         // Assert
//         Assert.IsInstanceOfType<IError>(error);
//     }

//     [TestMethod]
//     public void DatabaseError_Immutability_OriginalUnchanged()
//     {
//         // Arrange
//         var original = new DatabaseError("Original");

//         // Act
//         var modified = original
//             .WithQuery("SELECT * FROM Users")
//             .WithRetryCount(3);

//         // Assert
//         Assert.AreEqual("Original", original.Message);
//         Assert.IsTrue(original.Tags.IsEmpty);
        
//         Assert.AreEqual(2, modified.Tags.Count);
//     }

//     #endregion

//     #region NotFoundError Tests

//     [TestMethod]
//     public void NotFoundError_Entity_CreatesCorrectError()
//     {
//         // Act
//         var error = NotFoundError.Entity("User", "user-123");

//         // Assert
//         Assert.AreEqual("User with id 'user-123' not found", error.Message);
//         Assert.AreEqual("User", error.Tags["EntityType"]);
//         Assert.AreEqual("user-123", error.Tags["EntityId"]);
//         Assert.AreEqual(404, error.Tags["StatusCode"]);
//     }

//     [TestMethod]
//     public void NotFoundError_User_CreatesUserNotFoundError()
//     {
//         // Act
//         var error = NotFoundError.User("user-456");

//         // Assert
//         Assert.AreEqual("User with id 'user-456' not found", error.Message);
//         Assert.AreEqual("User", error.Tags["EntityType"]);
//         Assert.AreEqual("user-456", error.Tags["EntityId"]);
//         Assert.AreEqual(404, error.Tags["StatusCode"]);
//     }

//     [TestMethod]
//     public void NotFoundError_Order_CreatesOrderNotFoundError()
//     {
//         // Act
//         var error = NotFoundError.Order("ORD-001");

//         // Assert
//         Assert.AreEqual("Order with id 'ORD-001' not found", error.Message);
//         Assert.AreEqual("Order", error.Tags["EntityType"]);
//         Assert.AreEqual("ORD-001", error.Tags["EntityId"]);
//     }

//     [TestMethod]
//     public void NotFoundError_FactoryMethod_CanBeExtendedWithFluentAPI()
//     {
//         // Act
//         var error = NotFoundError.User("user-789")
//             .WithTags("RequestId", "req-123")
//             .WithTags("Timestamp", DateTime.UtcNow);

//         // Assert
//         Assert.AreEqual(5, error.Tags.Count); // EntityType, EntityId, StatusCode, RequestId, Timestamp
//         Assert.IsTrue(error.Tags.ContainsKey("RequestId"));
//         Assert.IsTrue(error.Tags.ContainsKey("Timestamp"));
//     }

//     [TestMethod]
//     public void NotFoundError_UsedInResult_Works()
//     {
//         // Arrange
//         var error = NotFoundError.User("user-999");

//         // Act
//         var result = Result<string>.Fail(error);

//         // Assert
//         Assert.IsTrue(result.IsFailed);
//         Assert.AreEqual("User with id 'user-999' not found", result.Errors[0].Message);
//         Assert.AreEqual(404, result.Errors[0].Tags["StatusCode"]);
//     }

//     #endregion

//     #region Authorization Error Tests

//     [TestMethod]
//     public void AuthorizationError_MissingPermission_CreatesCorrectError()
//     {
//         // Act
//         var error = AuthorizationError.MissingPermission("user-123", "admin:write");

//         // Assert
//         Assert.AreEqual("User 'user-123' lacks permission 'admin:write'", error.Message);
//         Assert.AreEqual("user-123", error.Tags["UserId"]);
//         Assert.AreEqual("admin:write", error.Tags["RequiredPermission"]);
//         Assert.AreEqual(403, error.Tags["StatusCode"]);
//     }

//     [TestMethod]
//     public void AuthorizationError_Forbidden_CreatesCorrectError()
//     {
//         // Act
//         var error = AuthorizationError.Forbidden("/admin/users");

//         // Assert
//         Assert.AreEqual("Access to '/admin/users' is forbidden", error.Message);
//         Assert.AreEqual("/admin/users", error.Tags["Resource"]);
//         Assert.AreEqual(403, error.Tags["StatusCode"]);
//     }

//     [TestMethod]
//     public void AuthorizationError_CanBeExtended_WithAdditionalContext()
//     {
//         // Act
//         var error = AuthorizationError.MissingPermission("user-456", "delete:orders")
//             .WithTags("IPAddress", "192.168.1.1")
//             .WithTags("Timestamp", DateTime.UtcNow)
//             .WithTags("Reason", "User role insufficient");

//         // Assert
//         Assert.AreEqual(6, error.Tags.Count);
//         Assert.IsTrue(error.Tags.ContainsKey("IPAddress"));
//         Assert.IsTrue(error.Tags.ContainsKey("Reason"));
//     }

//     [TestMethod]
//     public void AuthorizationError_UsedInWorkflow_Works()
//     {
//         // Arrange & Act
//         var result = Result<string>.Ok("user-789")
//             .Bind(userId =>
//             {
//                 // Simulate permission check
//                 if (userId != "admin-user")
//                 {
//                     return Result<string>.Fail(
//                         AuthorizationError.MissingPermission(userId, "admin:access")
//                     );
//                 }
//                 return Result<string>.Ok(userId);
//             });

//         // Assert
//         Assert.IsTrue(result.IsFailed);
//         Assert.AreEqual(403, result.Errors[0].Tags["StatusCode"]);
//     }

//     #endregion

//     #region Integration Tests - Multiple Custom Errors

//     [TestMethod]
//     public void MultipleCustomErrors_CanCoexist_InSameResult()
//     {
//         // Arrange
//         var validationError = new ValidationError("Email", "Invalid format");
//         var dbError = new DatabaseError("Connection failed");

//         // Act
//         var result = Result.Ok()
//             .WithError(validationError)
//             .WithError(dbError);

//         // Assert
//         Assert.IsTrue(result.IsFailed);
//         Assert.AreEqual(2, result.Errors.Count);
//         Assert.IsInstanceOfType<ValidationError>(result.Errors[0]);
//         Assert.IsInstanceOfType<DatabaseError>(result.Errors[1]);
//     }

//     [TestMethod]
//     public void CustomErrors_DifferentTypes_MaintainIndependence()
//     {
//         // Arrange
//         var notFoundError = NotFoundError.User("user-1");
//         var authError = AuthorizationError.Forbidden("/admin");

//         // Act
//         var result1 = Result<string>.Fail(notFoundError);
//         var result2 = Result<string>.Fail(authError);

//         // Assert
//         Assert.AreEqual(404, result1.Errors[0].Tags["StatusCode"]);
//         Assert.AreEqual(403, result2.Errors[0].Tags["StatusCode"]);
        
//         Assert.IsInstanceOfType<NotFoundError>(result1.Errors[0]);
//         Assert.IsInstanceOfType<AuthorizationError>(result2.Errors[0]);
//     }

//     #endregion

//     #region Real-World Usage Scenarios

//     [TestMethod]
//     public void RealWorld_ValidationPipeline_MultipleValidationErrors()
//     {
//         // Arrange
//         var emailError = new ValidationError("Email", "Invalid format");
//         var ageError = new ValidationError("Age", "Must be 18 or older");
//         var phoneError = new ValidationError("Phone", "Required field");

//         // Act
//         var result = Result<string>.Ok("user-data")
//             .WithError(emailError)
//             .WithError(ageError)
//             .WithError(phoneError);

//         // Assert
//         Assert.IsTrue(result.IsFailed);
//         Assert.AreEqual(3, result.Errors.Count);
//         Assert.IsTrue(result.Errors.All(e => e is ValidationError));
//     }

//     [TestMethod]
//     public void RealWorld_DatabaseOperation_WithRetries()
//     {
//         // Act
//         var error = new DatabaseError("Connection timeout after retries")
//             .WithQuery("SELECT * FROM Users WHERE Id = @id")
//             .WithRetryCount(3)
//             .WithTags("Server", "db-primary")
//             .WithTags("Database", "Production")
//             .WithTags("ConnectionTimeout", 30)
//             .WithTags("LastAttempt", DateTime.UtcNow);

//         var result = Result<string>.Fail(error);

//         // Assert
//         Assert.IsTrue(result.IsFailed);
//         var dbError = result.Errors[0] as DatabaseError;
//         Assert.IsNotNull(dbError);
//         Assert.AreEqual(3, dbError!.Tags["RetryCount"]);
//         Assert.IsTrue(dbError.Tags.ContainsKey("Query"));
//     }

//     [TestMethod]
//     public void RealWorld_APIEndpoint_NotFoundWithContext()
//     {
//         // Act
//         var error = NotFoundError.Order("ORD-12345")
//             .WithTags("RequestId", "req-abc-123")
//             .WithTags("UserId", "user-456")
//             .WithTags("Timestamp", DateTime.UtcNow)
//             .WithTags("Endpoint", "/api/orders/ORD-12345");

//         var result = Result<string>.Fail(error);

//         // Assert
//         Assert.IsTrue(result.IsFailed);
//         Assert.AreEqual(404, result.Errors[0].Tags["StatusCode"]);
//         Assert.IsTrue(result.Errors[0].Tags.ContainsKey("RequestId"));
//         Assert.AreEqual("/api/orders/ORD-12345", result.Errors[0].Tags["Endpoint"]);
//     }

//     [TestMethod]
//     public void RealWorld_Authorization_AuditTrail()
//     {
//         // Act
//         var error = AuthorizationError.MissingPermission("user-789", "delete:users")
//             .WithTags("IPAddress", "192.168.1.100")
//             .WithTags("UserAgent", "Mozilla/5.0")
//             .WithTags("SessionId", "session-xyz")
//             .WithTags("Timestamp", DateTime.UtcNow)
//             .WithTags("AuditId", Guid.NewGuid());

//         // Assert
//         Assert.AreEqual(7, error.Tags.Count);
//         Assert.IsTrue(error.Tags.ContainsKey("AuditId"));
//         Assert.IsTrue(error.Tags.ContainsKey("IPAddress"));
//     }

//     #endregion

//     #region Inheritance and Type System Tests

//     [TestMethod]
//     public void CustomErrors_AllImplementIError()
//     {
//         // Arrange
//         var validation = new ValidationError("Field", "Message");
//         var database = new DatabaseError("Message");
//         var notFound = NotFoundError.User("user-1");
//         var auth = AuthorizationError.Forbidden("resource");

//         // Assert
//         Assert.IsInstanceOfType<IError>(validation);
//         Assert.IsInstanceOfType<IError>(database);
//         Assert.IsInstanceOfType<IError>(notFound);
//         Assert.IsInstanceOfType<IError>(auth);
//     }

//     [TestMethod]
//     public void CustomErrors_CanBeTreatedAsIReason()
//     {
//         // Arrange
//         IError[] errors = new IError[]
//         {
//             new ValidationError("Field", "Message"),
//             new DatabaseError("DB Error"),
//             NotFoundError.User("user-1"),
//             AuthorizationError.Forbidden("res")
//         };

//         // Act
//         var messages = errors.Select(e => e.Message).ToList();

//         // Assert
//         Assert.AreEqual(4, messages.Count);
//         Assert.IsTrue(messages.All(m => !string.IsNullOrEmpty(m)));
//     }

//     [TestMethod]
//     public void CustomErrors_TypeChecking_Works()
//     {
//         // Arrange
//         var result = Result.Ok()
//             .WithError(new ValidationError("F", "M"))
//             .WithError(new DatabaseError("DB"));

//         // Act
//         var hasValidationError = result.Errors.Any(e => e is ValidationError);
//         var hasDatabaseError = result.Errors.Any(e => e is DatabaseError);
//         var hasNotFoundError = result.Errors.Any(e => e is NotFoundError);

//         // Assert
//         Assert.IsTrue(hasValidationError);
//         Assert.IsTrue(hasDatabaseError);
//         Assert.IsFalse(hasNotFoundError);
//     }

//     #endregion

//     #region Immutability Verification

//     [TestMethod]
//     public void CustomErrors_Immutability_DatabaseError()
//     {
//         // Arrange
//         var original = new DatabaseError("Original")
//             .WithQuery("SELECT 1");

//         // Act
//         var modified = original
//             .WithRetryCount(5)
//             .WithMessage("Modified");

//         // Assert
//         Assert.AreEqual("Original", original.Message);
//         Assert.AreEqual(1, original.Tags.Count); // Only Query
        
//         Assert.AreEqual("Modified", modified.Message);
//         Assert.AreEqual(2, modified.Tags.Count); // Query + RetryCount
//     }

//     [TestMethod]
//     public void CustomErrors_Immutability_NotFoundError()
//     {
//         // Arrange
//         var original = NotFoundError.User("user-1");

//         // Act
//         var modified = original.WithTags("Extra", "Info");

//         // Assert
//         Assert.AreEqual(3, original.Tags.Count); // EntityType, EntityId, StatusCode
//         Assert.AreEqual(4, modified.Tags.Count); // + Extra
//         Assert.IsFalse(original.Tags.ContainsKey("Extra"));
//         Assert.IsTrue(modified.Tags.ContainsKey("Extra"));
//     }

//     #endregion
// }