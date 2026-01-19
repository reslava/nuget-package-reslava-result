using System.Collections.Immutable;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Samples.Console;

/// <summary>
/// Demonstrates real-world scenarios using the Result pattern.
/// </summary>
public static class RealWorldScenariosSamples
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Real World Scenarios ===\n");

        await UserRegistrationScenario();
        await OrderProcessingScenario();
        await ApiClientScenario();
        await FileProcessingScenario();
        await DatabaseOperationScenario();
        await PaymentProcessingScenario();
        await EmailServiceScenario();
        await CacheScenario();

        System.Console.WriteLine("\n=== Real World Scenarios Complete ===\n");
    }

    #region User Registration Scenario

    private static async Task UserRegistrationScenario()
    {
        System.Console.WriteLine("--- User Registration Scenario ---");

        var request = new RegisterUserRequest
        {
            Email = "alice@example.com",
            Password = "SecureP@ss123",
            Name = "Alice Smith",
            Age = 25
        };

        var result = await RegisterUserAsync(request);

        result.Match(
            onSuccess: user =>
            {
                System.Console.WriteLine($"✓ User registered successfully!");
                System.Console.WriteLine($"  ID: {user.Id}");
                System.Console.WriteLine($"  Name: {user.Name}");
                System.Console.WriteLine($"  Email: {user.Email}");
                System.Console.WriteLine($"  Steps completed: {result.Successes.Count}");
                foreach (var success in result.Successes)
                {
                    System.Console.WriteLine($"    ✓ {success.Message}");
                }
            },
            onFailure: errors =>
            {
                System.Console.WriteLine($"✗ Registration failed: {errors.Count} error(s)");
                foreach (var error in errors)
                {
                    System.Console.WriteLine($"    ✗ {error.Message}");
                }
            }
        );

        // Invalid registration
        var invalidRequest = new RegisterUserRequest
        {
            Email = "invalid",
            Password = "weak",
            Name = "B",
            Age = 15
        };

        var invalidResult = await RegisterUserAsync(invalidRequest);
        System.Console.WriteLine($"\nInvalid registration: {invalidResult.Errors.Count} errors");
        foreach (var error in invalidResult.Errors)
        {
            System.Console.WriteLine($"  ✗ {error.Message}");
        }

        System.Console.WriteLine();
    }

    private static async Task<Result<User>> RegisterUserAsync(RegisterUserRequest request)
    {
        return await Result<RegisterUserRequest>.Ok(request, "Request received")
            .WithSuccess("Starting validation")
            // Validate email
            .Ensure(
                r => !string.IsNullOrWhiteSpace(r.Email),
                new ValidationError("Email", "Required field")
            )
            .Ensure(
                r => r.Email.Contains("@") && r.Email.Contains("."),
                new ValidationError("Email", "Invalid format")
            )
            // Validate password
            .Ensure(
                (r => r.Password.Length >= 8, new ValidationError("Password", "Min 8 characters")),
                (r => r.Password.Any(char.IsDigit), new ValidationError("Password", "Requires digit")),
                (r => r.Password.Any(char.IsUpper), new ValidationError("Password", "Requires uppercase")),
                (r => r.Password.Any(c => "!@#$%^&*".Contains(c)), new ValidationError("Password", "Requires special char"))
            )
            // Validate name
            .Ensure(
                r => r.Name.Length >= 2,
                new ValidationError("Name", "Too short")
            )
            // Validate age
            .Ensure(
                r => r.Age >= 18,
                new BusinessRuleError("MinimumAge", "Must be 18 or older")
                    .WithTag("MinimumAge", 18)
            )
            .Tap(r => System.Console.WriteLine("  Validation complete"))
            // Check email uniqueness
            .EnsureAsync(
                async r => await IsEmailUniqueAsync(r.Email),
                new BusinessRuleError("EmailExists", "Email already registered")
            )
            .TapAsync(r => System.Console.WriteLine("  Email uniqueness verified"))
            // Create user
            .MapAsync(async r =>
            {
                await Task.Delay(10); // Simulate hashing
                return new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = r.Name,
                    Email = r.Email,
                    PasswordHash = HashPassword(r.Password),
                    CreatedAt = DateTime.UtcNow
                };
            })
            .WithSuccess("User created")
            .TapAsync(u => System.Console.WriteLine($"  User ID generated: {u.Id}"))
            // Save to database
            .BindAsync(async u => await SaveUserToDatabaseAsync(u))
            .TapAsync(u => System.Console.WriteLine("  Saved to database"))
            // Send welcome email
            .TapAsync(async u => await SendWelcomeEmailAsync(u.Email))
            .WithSuccess("Welcome email sent");
    }

    #endregion

    #region Order Processing Scenario

    private static async Task OrderProcessingScenario()
    {
        System.Console.WriteLine("--- Order Processing Scenario ---");

        var order = new CreateOrderRequest
        {
            CustomerId = "customer-123",
            Items = new[]
            {
                new OrderItem { ProductId = "PROD-1", Quantity = 2, Price = 29.99m },
                new OrderItem { ProductId = "PROD-2", Quantity = 1, Price = 49.99m }
            },
            ShippingAddress = "123 Main St, City, State 12345"
        };

        var result = await ProcessOrderAsync(order);

        result.Match(
            onSuccess: processedOrder =>
            {
                System.Console.WriteLine($"✓ Order processed successfully!");
                System.Console.WriteLine($"  Order ID: {processedOrder.OrderId}");
                System.Console.WriteLine($"  Total: ${processedOrder.Total}");
                System.Console.WriteLine($"  Status: {processedOrder.Status}");
            },
            onFailure: errors =>
            {
                System.Console.WriteLine($"✗ Order processing failed:");
                foreach (var error in errors)
                {
                    System.Console.WriteLine($"    {error.Message}");
                }
            }
        );

        System.Console.WriteLine();
    }

    private static async Task<Result<ProcessedOrder>> ProcessOrderAsync(CreateOrderRequest request)
    {
        return await Result<CreateOrderRequest>.Ok(request, "Order received")
            // Validate order
            .Ensure(r => !string.IsNullOrWhiteSpace(r.CustomerId), "Customer ID required")
            .Ensure(r => r.Items?.Length > 0, "Order must have items")
            .Ensure(r => !string.IsNullOrWhiteSpace(r.ShippingAddress), "Shipping address required")
            .WithSuccess("Order validated")
            // Verify customer
            .BindAsync(async r => await VerifyCustomerAsync(r.CustomerId)
                .Map(_ => r))
            .WithSuccess("Customer verified")
            // Check inventory
            .BindAsync(async r =>
            {
                foreach (var item in r.Items!)
                {
                    var available = await CheckInventoryAsync(item.ProductId, item.Quantity);
                    if (!available)
                    {
                        return Result<CreateOrderRequest>.Fail(
                            new InventoryError("OUT_OF_STOCK", "Insufficient inventory")
                                .WithProductId(item.ProductId)
                                .WithRequestedQuantity(item.Quantity)
                        );
                    }
                }
                return Result<CreateOrderRequest>.Ok(r);
            })
            .WithSuccess("Inventory confirmed")
            // Calculate total
            .Map(r => new
            {
                Request = r,
                Total = r.Items!.Sum(i => i.Price * i.Quantity)
            })
            // Authorize payment
            .BindAsync(async data =>
            {
                var paymentResult = await AuthorizePaymentAsync(data.Request.CustomerId, data.Total);
                return paymentResult.Map(_ => data);
            })
            .WithSuccess("Payment authorized")
            // Reserve inventory
            .TapAsync(async data =>
            {
                foreach (var item in data.Request.Items!)
                {
                    await ReserveInventoryAsync(item.ProductId, item.Quantity);
                }
            })
            .WithSuccess("Inventory reserved")
            // Create order record
            .Map(data => new ProcessedOrder
            {
                OrderId = Guid.NewGuid().ToString(),
                CustomerId = data.Request.CustomerId,
                Items = data.Request.Items!.ToList(),
                Total = data.Total,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            })
            .WithSuccess("Order created");
    }

    #endregion

    #region API Client Scenario

    private static async Task ApiClientScenario()
    {
        System.Console.WriteLine("--- API Client Scenario ---");

        var userId = "user-123";
        var result = await FetchUserFromApiAsync(userId);

        result.Match(
            onSuccess: user =>
            {
                System.Console.WriteLine($"✓ User fetched from API:");
                System.Console.WriteLine($"  Name: {user.Name}");
                System.Console.WriteLine($"  Email: {user.Email}");
            },
            onFailure: errors =>
            {
                System.Console.WriteLine($"✗ API call failed:");
                foreach (var error in errors)
                {
                    System.Console.WriteLine($"    {error.Message}");
                    if (error.Tags.ContainsKey("StatusCode"))
                    {
                        System.Console.WriteLine($"    Status: {error.Tags["StatusCode"]}");
                    }
                    if (error.Tags.ContainsKey("RetryAfter"))
                    {
                        System.Console.WriteLine($"    Retry after: {error.Tags["RetryAfter"]}s");
                    }
                }
            }
        );

        System.Console.WriteLine();
    }

    private static async Task<Result<User>> FetchUserFromApiAsync(string userId)
    {
        return await Result<int>.TryAsync(
            async () =>
            {
                await Task.Delay(10); // Simulate API call

                // Simulate different responses
                var random = new Random().Next(0, 10);
                if (random < 7) // 70% success
                {
                    return 200;
                }
                else if (random < 9) // 20% not found
                {
                    return 404;
                }
                else // 10% rate limit
                {
                    return 429;
                }
            },
            ex => new ApiError($"API call failed: {ex.Message}")
                .WithEndpoint($"/api/users/{userId}")
                .WithHttpMethod("GET")
        )
        .BindAsync(async statusCode =>
        {
            if (statusCode == 200)
            {
                return Result<User>.Ok(new User
                {
                    Id = userId,
                    Name = "John Doe",
                    Email = "john@example.com"
                });
            }
            else if (statusCode == 404)
            {
                return Result<User>.Fail(
                    new NotFoundError("User", userId)
                );
            }
            else if (statusCode == 429)
            {
                return Result<User>.Fail(
                    new ApiError("Rate limit exceeded")
                        .WithEndpoint($"/api/users/{userId}")
                        .WithStatusCode(429)
                        .WithRetryAfter(60)
                );
            }
            else
            {
                return Result<User>.Fail($"Unexpected status code: {statusCode}");
            }
        });
    }

    #endregion

    #region File Processing Scenario

    private static async Task FileProcessingScenario()
    {
        System.Console.WriteLine("--- File Processing Scenario ---");

        var filePath = "data.csv";
        var result = await ProcessFileAsync(filePath);

        result.Match(
            onSuccess: data =>
            {
                System.Console.WriteLine($"✓ File processed successfully:");
                System.Console.WriteLine($"  Records: {data.RecordCount}");
                System.Console.WriteLine($"  Valid: {data.ValidRecords}");
                System.Console.WriteLine($"  Invalid: {data.InvalidRecords}");
            },
            onFailure: errors =>
            {
                System.Console.WriteLine($"✗ File processing failed:");
                foreach (var error in errors)
                {
                    System.Console.WriteLine($"    {error.Message}");
                }
            }
        );

        System.Console.WriteLine();
    }

    private static async Task<Result<FileProcessingResult>> ProcessFileAsync(string filePath)
    {
        return await Result<string>.Ok(filePath, "File path received")
            // Validate file exists
            .Ensure(
                path => !string.IsNullOrWhiteSpace(path),
                "File path required"
            )
            .Ensure(
                path => path.EndsWith(".csv"),
                "Only CSV files supported"
            )
            .WithSuccess("File path validated")
            // Read file
            .BindAsync(async path =>
            {
                return await Result<string[]>.TryAsync(
                    async () =>
                    {
                        await Task.Delay(10); // Simulate file read
                        // Simulate file content
                        return new[]
                        {
                            "Name,Email,Age",
                            "Alice,alice@example.com,25",
                            "Bob,bob@example.com,30",
                            "Charlie,invalid-email,28"
                        };
                    },
                    ex => new FileError($"Failed to read file: {ex.Message}")
                        .WithFilePath(path)
                );
            })
            .WithSuccess("File read successfully")
            // Parse records
            .Map(lines =>
            {
                var records = new List<CsvRecord>();
                var invalidCount = 0;

                for (int i = 1; i < lines.Length; i++) // Skip header
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length == 3 && parts[1].Contains("@"))
                    {
                        records.Add(new CsvRecord
                        {
                            Name = parts[0],
                            Email = parts[1],
                            Age = int.TryParse(parts[2], out var age) ? age : 0
                        });
                    }
                    else
                    {
                        invalidCount++;
                    }
                }

                return new FileProcessingResult
                {
                    RecordCount = lines.Length - 1,
                    ValidRecords = records.Count,
                    InvalidRecords = invalidCount,
                    Records = records
                };
            })
            .WithSuccess("Records parsed");
    }

    #endregion

    #region Database Operation Scenario

    private static async Task DatabaseOperationScenario()
    {
        System.Console.WriteLine("--- Database Operation Scenario ---");

        var userId = "user-456";
        var result = await UpdateUserWithRetryAsync(userId, new { Email = "newemail@example.com" });

        result.Match(
            onSuccess: user =>
            {
                System.Console.WriteLine($"✓ User updated successfully:");
                System.Console.WriteLine($"  ID: {user.Id}");
                System.Console.WriteLine($"  Email: {user.Email}");
            },
            onFailure: errors =>
            {
                System.Console.WriteLine($"✗ Database operation failed:");
                foreach (var error in errors)
                {
                    System.Console.WriteLine($"    {error.Message}");
                    if (error is DatabaseError dbError && dbError.Tags.ContainsKey("RetryCount"))
                    {
                        System.Console.WriteLine($"    Retries attempted: {dbError.Tags["RetryCount"]}");
                    }
                }
            }
        );

        System.Console.WriteLine();
    }

    private static async Task<Result<User>> UpdateUserWithRetryAsync(string userId, object updates)
    {
        const int maxRetries = 3;
        Result<User>? lastResult = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            lastResult = await Result<string>.Ok(userId)
                .BindAsync(async id => await GetUserFromDatabaseAsync(id))
                .BindAsync(async user =>
                {
                    return (await Result<object>.TryAsync(
                        async () =>
                        {
                            await Task.Delay(10); // Simulate update

                            // Simulate occasional failures
                            if (new Random().Next(0, 10) < 3 && attempt < maxRetries)
                            {
                                throw new Exception("Connection timeout");
                            }
                            return new object(); // Return dummy value
                        },
                        ex => new DatabaseError($"Update failed: {ex.Message}")
                            .WithQuery($"UPDATE Users SET Email = @email WHERE Id = @id")
                            .WithRetryCount(attempt)
                    ))
                    .Map(_ => user);
                });

            if (lastResult.IsSuccess)
            {
                System.Console.WriteLine($"  Succeeded on attempt {attempt}");
                break;
            }
            else if (attempt < maxRetries)
            {
                System.Console.WriteLine($"  Attempt {attempt} failed, retrying...");
                await Task.Delay(100 * attempt); // Exponential backoff
            }
        }

        return lastResult!;
    }

    #endregion

    #region Payment Processing Scenario

    private static async Task PaymentProcessingScenario()
    {
        System.Console.WriteLine("--- Payment Processing Scenario ---");

        var payment = new PaymentRequest
        {
            Amount = 99.99m,
            Currency = "USD",
            CardNumber = "4111111111111111",
            CardExpiry = "12/25",
            Cvv = "123"
        };

        var result = await ProcessPaymentAsync(payment);

        result.Match(
            onSuccess: transaction =>
            {
                System.Console.WriteLine($"✓ Payment processed:");
                System.Console.WriteLine($"  Transaction ID: {transaction.TransactionId}");
                System.Console.WriteLine($"  Amount: ${transaction.Amount}");
                System.Console.WriteLine($"  Status: {transaction.Status}");
            },
            onFailure: errors =>
            {
                System.Console.WriteLine($"✗ Payment failed:");
                foreach (var error in errors)
                {
                    System.Console.WriteLine($"    {error.Message}");
                }
            }
        );

        System.Console.WriteLine();
    }

    private static async Task<Result<PaymentTransaction>> ProcessPaymentAsync(PaymentRequest request)
    {
        return await Result<PaymentRequest>.Ok(request, "Payment request received")
            // Validate payment details
            .Ensure(r => r.Amount > 0, "Amount must be positive")
            .Ensure(r => r.Amount <= 10000, "Amount exceeds maximum")
            .Ensure(r => r.CardNumber.Length == 16, "Invalid card number")
            .WithSuccess("Payment validated")
            // Fraud check
            .BindAsync(async r =>
            {
                var fraudScore = await CalculateFraudScoreAsync(r);
                if (fraudScore > 0.8)
                {
                    return Result<PaymentRequest>.Fail(
                        new FraudDetectedError("High fraud risk detected")
                            .WithRiskScore(fraudScore)
                            .WithTag("Reason", "Unusual amount pattern")
                    );
                }
                return Result<PaymentRequest>.Ok(r);
            })
            .WithSuccess("Fraud check passed")
            // Process payment
            .MapAsync(async r =>
            {
                await Task.Delay(10); // Simulate payment processing
                return new PaymentTransaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    Amount = r.Amount,
                    Currency = r.Currency,
                    Status = "Completed",
                    ProcessedAt = DateTime.UtcNow
                };
            })
            .WithSuccess("Payment processed");
    }

    #endregion

    #region Email Service Scenario

    private static async Task EmailServiceScenario()
    {
        System.Console.WriteLine("--- Email Service Scenario ---");

        var recipients = new[] { "user1@example.com", "user2@example.com", "user3@example.com" };
        var result = await SendBulkEmailAsync(recipients, "Welcome!", "Welcome to our service!");

        result.Match(
            onSuccess: emailResult =>
            {
                System.Console.WriteLine($"✓ Emails sent:");
                System.Console.WriteLine($"  Total: {emailResult.TotalRecipients}");
                System.Console.WriteLine($"  Successful: {emailResult.SuccessCount}");
                System.Console.WriteLine($"  Failed: {emailResult.FailureCount}");
            },
            onFailure: errors =>
            {
                System.Console.WriteLine($"✗ Email service failed:");
                foreach (var error in errors)
                {
                    System.Console.WriteLine($"    {error.Message}");
                }
            }
        );

        System.Console.WriteLine();
    }

    private static async Task<Result<EmailSendResult>> SendBulkEmailAsync(
        string[] recipients,
        string subject,
        string body)
    {
        var tasks = recipients.Select(email =>
            Result.TryAsync(
                async () =>
                {
                    await Task.Delay(10); // Simulate sending
                    // Simulate 90% success rate
                    if (new Random().Next(0, 10) < 9)
                    {
                        return;
                    }
                    throw new Exception($"Failed to send to {email}");
                },
                ex => new EmailError($"Email send failed: {ex.Message}")
                    .WithRecipient(email)
            )
        );

        var results = await Task.WhenAll(tasks);
        var failures = results.Where(r => r.IsFailed).ToList();

        if (failures.Count == recipients.Length)
        {
            // All failed
            return Result<EmailSendResult>.Fail("All emails failed to send");
        }

        // Partial or complete success
        return Result<EmailSendResult>.Ok(new EmailSendResult
        {
            TotalRecipients = recipients.Length,
            SuccessCount = results.Count(r => r.IsSuccess),
            FailureCount = failures.Count
        }, failures.Any() ? "Some emails failed" : "All emails sent");
    }

    #endregion

    #region Cache Scenario

    private static async Task CacheScenario()
    {
        System.Console.WriteLine("--- Cache Scenario ---");

        var userId = "user-789";
        var result = await GetUserWithCacheAsync(userId);

        result.Match(
            onSuccess: user =>
            {
                System.Console.WriteLine($"✓ User retrieved:");
                System.Console.WriteLine($"  Name: {user.Name}");
                System.Console.WriteLine($"  Source: {(result.Successes.Any(s => s.Message.Contains("cache")) ? "Cache" : "Database")}");
            },
            onFailure: errors =>
            {
                System.Console.WriteLine($"✗ User retrieval failed:");
                foreach (var error in errors)
                {
                    System.Console.WriteLine($"    {error.Message}");
                }
            }
        );

        System.Console.WriteLine();
    }

    private static async Task<Result<User>> GetUserWithCacheAsync(string userId)
    {
        // Try cache first
        var cacheResult = await GetFromCacheAsync<User>($"user:{userId}");

        if (cacheResult.IsSuccess)
        {
            return cacheResult.WithSuccess("Retrieved from cache");
        }

        // Cache miss - get from database
        return await GetUserFromDatabaseAsync(userId)
            .TapAsync(async user =>
            {
                // Store in cache
                await SetCacheAsync($"user:{userId}", user, TimeSpan.FromMinutes(5));
                System.Console.WriteLine("  Cached for 5 minutes");
            })
            .WithSuccess("Retrieved from database");
    }

    #endregion

    #region Helper Methods

    private static async Task<bool> IsEmailUniqueAsync(string email)
    {
        await Task.Delay(10);
        return !email.Contains("duplicate");
    }

    private static string HashPassword(string password)
    {
        return $"HASHED_{password}";
    }

    private static async Task<Result<User>> SaveUserToDatabaseAsync(User user)
    {
        await Task.Delay(10);
        return Result<User>.Ok(user);
    }

    private static async Task SendWelcomeEmailAsync(string email)
    {
        await Task.Delay(10);
        System.Console.WriteLine($"  Welcome email sent to {email}");
    }

    private static async Task<Result<string>> VerifyCustomerAsync(string customerId)
    {
        await Task.Delay(10);
        return Result<string>.Ok(customerId);
    }

    private static async Task<bool> CheckInventoryAsync(string productId, int quantity)
    {
        await Task.Delay(10);
        return true; // Simulate availability
    }

    private static async Task<Result<string>> AuthorizePaymentAsync(string customerId, decimal amount)
    {
        await Task.Delay(10);
        return Result<string>.Ok("AUTHORIZED");
    }

    private static async Task ReserveInventoryAsync(string productId, int quantity)
    {
        await Task.Delay(10);
    }

    private static async Task<Result<User>> GetUserFromDatabaseAsync(string userId)
    {
        await Task.Delay(10);
        return Result<User>.Ok(new User
        {
            Id = userId,
            Name = "Database User",
            Email = "user@example.com"
        });
    }

    private static async Task<double> CalculateFraudScoreAsync(PaymentRequest request)
    {
        await Task.Delay(10);
        return 0.3; // Low risk
    }

    private static async Task<Result<T>> GetFromCacheAsync<T>(string key)
    {
        await Task.Delay(5);
        // Simulate cache miss
        return Result<T>.Fail($"Cache miss for key: {key}");
    }

    private static async Task SetCacheAsync<T>(string key, T value, TimeSpan expiry)
    {
        await Task.Delay(5);
    }

    #endregion

    #region Custom Error Types

    private class ValidationError : Error
    {
        public ValidationError(string field, string message)
            : base($"{field}: {message}")
        {
            WithTag("Field", field);
            WithTag("ErrorType", "Validation");
        }
    }

    private class BusinessRuleError : Error
    {
        public BusinessRuleError(string ruleCode, string message)
            : base(message)
        {
            WithTag("RuleCode", ruleCode);
            WithTag("ErrorType", "BusinessRule");
        }
    }

    private class NotFoundError : Error
    {
        public NotFoundError(string entityType, string entityId)
            : base($"{entityType} with id '{entityId}' not found")
        {
            WithTag("EntityType", entityType);
            WithTag("EntityId", entityId);
            WithTag("StatusCode", 404);
        }
    }

    private class ApiError : Reason<ApiError>, IError
    {
        public ApiError(string message) : base(message) { }
        public ApiError(string message, ImmutableDictionary<string, object> tags) : base(message, tags) { }

        public ApiError WithEndpoint(string endpoint) => WithTag("Endpoint", endpoint);
        public ApiError WithHttpMethod(string method) => WithTag("HttpMethod", method);
        public ApiError WithStatusCode(int code) => WithTag("StatusCode", code);
        public ApiError WithRetryAfter(int seconds) => WithTag("RetryAfter", seconds);

        protected override ApiError CreateNew(string message, System.Collections.Immutable.ImmutableDictionary<string, object> tags)
        {
            return new ApiError(message, tags);
        }
    }

    private class InventoryError : Reason<InventoryError>, IError
    {
       public InventoryError(string code, string message) : base(message)
       {
           WithTag("ErrorCode", code);
       }
       public InventoryError(string code, string message, ImmutableDictionary<string, object> tags) : base(message, tags) 
       {
           WithTag("ErrorCode", code);
       }
       public InventoryError(string message, ImmutableDictionary<string, object> tags) : base(message, tags) { }


       public InventoryError WithProductId(string id) => WithTag("ProductId", id);
       public InventoryError WithRequestedQuantity(int qty) => WithTag("RequestedQuantity", qty);

       protected override InventoryError CreateNew(string message, ImmutableDictionary<string, object> tags)
       {
           return new InventoryError(message, tags);
       }
    }
    // // private class InventoryError : Reason<InventoryError>, IError
    // // {
    // //     // Private constructor - only factory methods can create instances
    // //     private InventoryError(string message, ImmutableDictionary<string, object> tags)
    // //         : base(message, tags) { }

    // //     // Factory methods for common scenarios
    // //     public static InventoryError OutOfStock(string productId, int requested, int available)
    // //     {
    // //         var tags = ImmutableDictionary<string, object>.Empty
    // //             .Add("ErrorCode", "OUT_OF_STOCK")
    // //             .Add("ProductId", productId)
    // //             .Add("RequestedQuantity", requested)
    // //             .Add("AvailableQuantity", available);

    // //         return new InventoryError(
    // //             $"Insufficient stock for {productId}. Requested: {requested}, Available: {available}",
    // //             tags);
    // //     }

    // //     public static InventoryError InvalidQuantity(string productId, int quantity)
    // //     {
    // //         var tags = ImmutableDictionary<string, object>.Empty
    // //             .Add("ErrorCode", "INVALID_QUANTITY")
    // //             .Add("ProductId", productId)
    // //             .Add("Quantity", quantity);

    // //         return new InventoryError(
    // //             $"Invalid quantity {quantity} for product {productId}",
    // //             tags);
    // //     }

    // //     public static InventoryError ProductNotFound(string productId)
    // //     {
    // //         var tags = ImmutableDictionary<string, object>.Empty
    // //             .Add("ErrorCode", "PRODUCT_NOT_FOUND")
    // //             .Add("ProductId", productId);

    // //         return new InventoryError(
    // //             $"Product {productId} not found in inventory",
    // //             tags);
    // //     }

    // //     // CRTP factory method
    // //     protected override InventoryError CreateNew(
    // //         string message,
    // //         ImmutableDictionary<string, object> tags)
    // //     {
    // //         return new InventoryError(message, tags);
    // //     }
    // // }

    // Usage:
    //////public Result<Order> ProcessOrder(string productId, int quantity)
    //////{
    //////    var available = GetAvailableQuantity(productId);

    //////    if (available < quantity)
    //////    {
    //////        return Result<Order>.Fail(
    //////            InventoryError.OutOfStock(productId, quantity, available)
    //////                .WithTag("WarehouseId", "WH-5")  // Can still add more tags
    //////        );
    //////    }

    //////    // ... rest of order processing
    //////}
    private class FileError : Reason<FileError>, IError
    {
        public FileError(string message) : base(message) { }

        private FileError(string message, ImmutableDictionary<string, object> tags) 
            : base(message, tags) { }

        protected override FileError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new FileError(message, tags);
        }

        public FileError WithFilePath(string path) => WithTag("FilePath", path);
    }

    private class DatabaseError : Reason<DatabaseError>, IError
    {
        public DatabaseError(string message) : base(message) { }

        private DatabaseError(string message, ImmutableDictionary<string, object> tags) 
            : base(message, tags) { }

        protected override DatabaseError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new DatabaseError(message, tags);
        }

        public DatabaseError WithQuery(string query) => WithTag("Query", query);
        public DatabaseError WithRetryCount(int count) => WithTag("RetryCount", count);
    }

    private class FraudDetectedError : Reason<FraudDetectedError>, IError
    {
        public FraudDetectedError(string message) : base(message) { }

        private FraudDetectedError(string message, ImmutableDictionary<string, object> tags) 
            : base(message, tags) { }

        protected override FraudDetectedError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new FraudDetectedError(message, tags);
        }

        public FraudDetectedError WithRiskScore(double score) => WithTag("RiskScore", score);
    }

    private class EmailError : Reason<EmailError>, IError
    {
        public EmailError(string message) : base(message) { }

        private EmailError(string message, ImmutableDictionary<string, object> tags) 
            : base(message, tags) { }

        protected override EmailError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new EmailError(message, tags);
        }

        public EmailError WithRecipient(string email) => WithTag("Recipient", email);
    }

    #endregion

    #region Data Models

    private class RegisterUserRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private class User
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    private class CreateOrderRequest
    {
        public string CustomerId { get; set; } = "";
        public OrderItem[]? Items { get; set; }
        public string ShippingAddress { get; set; } = "";
    }

    private class OrderItem
    {
        public string ProductId { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    private class ProcessedOrder
    {
        public string OrderId { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public List<OrderItem> Items { get; set; } = new();
        public decimal Total { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    private class FileProcessingResult
    {
        public int RecordCount { get; set; }
        public int ValidRecords { get; set; }
        public int InvalidRecords { get; set; }
        public List<CsvRecord> Records { get; set; } = new();
    }
    private class CsvRecord
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public int Age { get; set; }
    }

    private class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "";
        public string CardNumber { get; set; } = "";
        public string CardExpiry { get; set; } = "";
        public string Cvv { get; set; } = "";
    }

    private class PaymentTransaction
    {
        public string TransactionId { get; set; } = "";
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime ProcessedAt { get; set; }
    }

    private class EmailSendResult
    {
        public int TotalRecipients { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
    }

    #endregion
}
