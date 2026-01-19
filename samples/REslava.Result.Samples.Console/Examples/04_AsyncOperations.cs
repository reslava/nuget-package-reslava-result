using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Samples.Console;

/// <summary>
/// Demonstrates async operations with the Result pattern.
/// </summary>
public static class AsyncOperationsSamples
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Async Operations Samples ===\n");

        await TryAsyncPattern();
        await MapAsync();
        await BindAsync();
        await EnsureAsync();
        await TapAsync();
        await MatchAsync();
        await CombiningAsyncResults();
        await CompleteAsyncPipeline();

        System.Console.WriteLine("\n=== Async Operations Complete ===\n");
    }

    #region TryAsync Pattern

    private static async Task TryAsyncPattern()
    {
        System.Console.WriteLine("--- TryAsync Pattern ---");

        // TryAsync - success
        var result1 = await Result.TryAsync(async () =>
        {
            await Task.Delay(10);
            System.Console.WriteLine("  Async operation completed");
        });

        System.Console.WriteLine($"TryAsync success: {result1.IsSuccess}");

        // TryAsync - exception
        var result2 = await Result.TryAsync(async () =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Async operation failed");
        });

        System.Console.WriteLine($"TryAsync with exception: {result2.IsFailed}");
        if (result2.IsFailed)
        {
            System.Console.WriteLine($"  Error: {result2.Errors[0].Message}");
        }

        // TryAsync with custom error handler
        var result3 = await Result.TryAsync(
            async () =>
            {
                await Task.Delay(10);
                throw new HttpRequestException("Network error");
            },
            ex => new Error($"API call failed: {ex.Message}")
                .WithTag("ErrorType", "Network")
                .WithTag("Retryable", true)
        );

        System.Console.WriteLine($"\nTryAsync with custom handler: {result3.IsFailed}");
        if (result3.IsFailed)
        {
            System.Console.WriteLine($"  Message: {result3.Errors[0].Message}");
            System.Console.WriteLine($"  Retryable: {result3.Errors[0].Tags["Retryable"]}");
        }

        // Generic TryAsync
        var result4 = await Result<User>.TryAsync(async () =>
        {
            await Task.Delay(10);
            return new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
        });

        System.Console.WriteLine($"\nGeneric TryAsync: {result4.IsSuccess}");
        if (result4.IsSuccess)
        {
            System.Console.WriteLine($"  User: {result4.Value.Name}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region MapAsync

    private static async Task MapAsync()
    {
        System.Console.WriteLine("--- MapAsync ---");

        // Sync Map on sync result
        var result1 = Result<int>.Ok(10)
            .Map(x => x * 2);

        System.Console.WriteLine($"Sync Map: {result1.Value}");

        // Async Map on sync result
        var result2 = await Result<int>.Ok(10)
            .MapAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            });

        System.Console.WriteLine($"MapAsync on sync result: {result2.Value}");

        // Sync Map on async result
        var result3 = await GetUserAsync(1)
            .Map(u => u.Name);

        System.Console.WriteLine($"Sync Map on async result: {result3.Value}");

        // Async Map on async result
        var result4 = await GetUserAsync(1)
            .MapAsync(async u =>
            {
                await Task.Delay(10);
                return u.Name.ToUpper();
            });

        System.Console.WriteLine($"MapAsync on async result: {result4.Value}");

        // Chained MapAsync
        var result5 = await Result<int>.Ok(5)
            .MapAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            })
            .MapAsync(async x =>
            {
                await Task.Delay(10);
                return x + 10;
            })
            .MapAsync(async x =>
            {
                await Task.Delay(10);
                return $"Result: {x}";
            });

        System.Console.WriteLine($"Chained MapAsync: {result5.Value}");

        System.Console.WriteLine();
    }

    #endregion

    #region BindAsync

    private static async Task BindAsync()
    {
        System.Console.WriteLine("--- BindAsync ---");

        // Async Bind on sync result
        var result1 = await Result<int>.Ok(1)
            .BindAsync(async id =>
            {
                await Task.Delay(10);
                return await GetUserAsync(id);
            });

        System.Console.WriteLine($"BindAsync: {result1.IsSuccess}, User: {result1.Value?.Name}");

        // Async Bind on async result
        var result2 = await GetUserAsync(1)
            .BindAsync(async user => await ValidateUserAsync(user));

        System.Console.WriteLine($"BindAsync chain: {result2.IsSuccess}");

        // Complex async chain
        var result3 = await Result<int>.Ok(1)
            .BindAsync(async id => await GetUserAsync(id))
            .BindAsync(async user => await ValidateUserAsync(user))
            .BindAsync(async user => await SaveUserAsync(user));

        System.Console.WriteLine($"Complex async chain: {result3.IsSuccess}");
        if (result3.IsSuccess)
        {
            System.Console.WriteLine($"  Saved user: {result3.Value.Name}");
        }

        // Success reasons preserved through async chain
        var result4 = await Result<int>.Ok(1, "User ID validated")
            .WithSuccess("Ready to fetch")
            .BindAsync(async id => await GetUserAsync(id))
            .BindAsync(async user => await ValidateUserAsync(user));

        System.Console.WriteLine($"\nSuccess reasons preserved: {result4.Successes.Count}");
        foreach (var success in result4.Successes)
        {
            System.Console.WriteLine($"  ✓ {success.Message}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region EnsureAsync

    private static async Task EnsureAsync()
    {
        System.Console.WriteLine("--- EnsureAsync ---");

        // Sync Ensure on async result
        var result1 = await GetUserAsync(1)
            .EnsureAsync(
                u => u.Age >= 18,
                "Must be 18 or older"
            );

        System.Console.WriteLine($"Sync Ensure on async result: {result1.IsSuccess}");

        // Async Ensure on sync result
        var result2 = await Result<User>.Ok(new User { Id = 1, Name = "Alice", Email = "alice@example.com" })
            .EnsureAsync(
                async u => await IsEmailUniqueAsync(u.Email),
                "Email already exists"
            );

        System.Console.WriteLine($"Async Ensure on sync result: {result2.IsSuccess}");

        // Async Ensure on async result
        var result3 = await GetUserAsync(1)
            .EnsureAsync(
                async u => await IsUserActiveAsync(u.Id),
                "User is not active"
            );

        System.Console.WriteLine($"Async Ensure on async result: {result3.IsSuccess}");

        // Chained async validations
        var result4 = await GetUserAsync(1)
            .EnsureAsync(u => u.Age >= 18, "Must be 18+")
            .EnsureAsync(async u => await IsEmailUniqueAsync(u.Email), "Email exists")
            .EnsureAsync(async u => await IsUserActiveAsync(u.Id), "User inactive");

        System.Console.WriteLine($"Chained async validations: {result4.IsSuccess}");

        System.Console.WriteLine();
    }

    #endregion

    #region TapAsync

    private static async Task TapAsync()
    {
        System.Console.WriteLine("--- TapAsync ---");

        // Sync Tap on async result
        var result1 = await GetUserAsync(1)
            .TapAsync(u => System.Console.WriteLine($"  Processing user: {u.Name}"));

        System.Console.WriteLine($"Sync Tap on async result: {result1.IsSuccess}");

        // Async Tap on async result
        var result2 = await GetUserAsync(1)
            .TapAsync(async u =>
            {
                await Task.Delay(10);
                System.Console.WriteLine($"  Async processing user: {u.Name}");
            });

        System.Console.WriteLine($"Async Tap on async result: {result2.IsSuccess}");

        // Multiple Taps in pipeline
        var result3 = await GetUserAsync(1)
            .TapAsync(u => System.Console.WriteLine($"  1. Loaded: {u.Name}"))
            .TapAsync(async u =>
            {
                await Task.Delay(10);
                System.Console.WriteLine($"  2. Validated: {u.Email}");
            })
            .TapAsync(u => System.Console.WriteLine($"  3. Processing complete"));

        System.Console.WriteLine($"Multiple Taps: {result3.IsSuccess}");

        // TapOnFailure async
        var failedResult = Result<User>.Fail("User not found");
        await failedResult.TapOnFailureAsync(async error =>
        {
            await Task.Delay(10);
            System.Console.WriteLine($"  Error handler: {error.Message}");
        });

        System.Console.WriteLine();
    }

    #endregion

    #region MatchAsync

    private static async Task MatchAsync()
    {
        System.Console.WriteLine("--- MatchAsync ---");

        // Async Match on async result (with return value)
        var message1 = await GetUserAsync(1)
            .MatchAsync(
                onSuccess: async user =>
                {
                    await Task.Delay(10);
                    return $"Welcome, {user.Name}!";
                },
                onFailure: async errors =>
                {
                    await Task.Delay(10);
                    return $"Error: {errors[0].Message}";
                }
            );

        System.Console.WriteLine($"MatchAsync with return: {message1}");

        // Async Match (actions only)
        await GetUserAsync(1)
            .MatchAsync(
                onSuccess: async user =>
                {
                    await Task.Delay(10);
                    System.Console.WriteLine($"  Success: Loaded {user.Name}");
                },
                onFailure: async errors =>
                {
                    await Task.Delay(10);
                    System.Console.WriteLine($"  Failure: {errors[0].Message}");
                }
            );

        // Match on failed async result
        var failedResult = await Result<User>.TryAsync(async () =>
        {
            await Task.Delay(10);
            throw new Exception("User service unavailable");
        });

        var message2 = await failedResult.MatchAsync(
            onSuccess: async user =>
            {
                await Task.Delay(10);
                return $"User: {user.Name}";
            },
            onFailure: async errors =>
            {
                await Task.Delay(10);
                return $"Service error: {errors[0].Message}";
            }
        );

        System.Console.WriteLine($"Failed result Match: {message2}");

        System.Console.WriteLine();
    }

    #endregion

    #region Combining Async Results

    private static async Task CombiningAsyncResults()
    {
        System.Console.WriteLine("--- Combining Async Results ---");

        // CombineParallelAsync - all succeed
        var tasks1 = new[]
        {
            GetUserAsync(1),
            GetUserAsync(2),
            GetUserAsync(3)
        };

        var combined1 = await Result<User>.CombineParallelAsync(tasks1);

        System.Console.WriteLine($"CombineParallelAsync (success): {combined1.IsSuccess}");
        if (combined1.IsSuccess)
        {
            System.Console.WriteLine($"  Users loaded: {combined1.Value.Count()}");
            foreach (var user in combined1.Value)
            {
                System.Console.WriteLine($"    - {user.Name}");
            }
        }

        // CombineParallelAsync - with failure
        var tasks2 = new[]
        {
            GetUserAsync(1),
            Result<User>.TryAsync(async () =>
            {
                await Task.Delay(10);
                throw new Exception("User 2 not found");
            }),
            GetUserAsync(3)
        };

        var combined2 = await Result<User>.CombineParallelAsync(tasks2);

        System.Console.WriteLine($"\nCombineParallelAsync (with failure): {combined2.IsFailed}");
        if (combined2.IsFailed)
        {
            System.Console.WriteLine($"  Errors: {combined2.Errors.Count}");
            foreach (var error in combined2.Errors)
            {
                System.Console.WriteLine($"    ✗ {error.Message}");
            }
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Complete Async Pipeline

    private static async Task CompleteAsyncPipeline()
    {
        System.Console.WriteLine("--- Complete Async Pipeline ---");

        var userId = 1;

        var result = await Result<int>.Ok(userId, "User ID received")
            .WithSuccess("Request validated")
            .TapAsync(id => System.Console.WriteLine($"  Processing user ID: {id}"))
            .BindAsync(async id =>
            {
                System.Console.WriteLine("  Fetching user...");
                return await GetUserAsync(id);
            })
            .TapAsync(u => System.Console.WriteLine($"  User loaded: {u.Name}"))
            .EnsureAsync(u => u.Age >= 18, "Must be 18+")
            .EnsureAsync(async u => await IsEmailUniqueAsync(u.Email), "Email not unique")
            .EnsureAsync(async u => await IsUserActiveAsync(u.Id), "User not active")
            .TapAsync(u => System.Console.WriteLine($"  All validations passed"))
            .MapAsync(async u =>
            {
                await Task.Delay(10);
                return new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Status = "Active"
                };
            })
            .TapAsync(dto => System.Console.WriteLine($"  DTO created: {dto.Name}"))
            .BindAsync(async dto =>
            {
                System.Console.WriteLine("  Saving to database...");
                await Task.Delay(10);
                return Result<UserDto>.Ok(dto, "User saved successfully");
            });

        System.Console.WriteLine($"\nPipeline result: {result.IsSuccess}");
        if (result.IsSuccess)
        {
            System.Console.WriteLine($"  Final DTO: {result.Value.Name} ({result.Value.Status})");
            System.Console.WriteLine($"  Success reasons: {result.Successes.Count}");
            foreach (var success in result.Successes)
            {
                System.Console.WriteLine($"    ✓ {success.Message}");
            }
        }

        // Failed pipeline (stops at first error)
        System.Console.WriteLine("\n--- Failed Pipeline ---");

        var failedResult = await Result<int>.Ok(999)
            .TapAsync(id => System.Console.WriteLine($"  Processing ID: {id}"))
            .BindAsync(async id =>
            {
                System.Console.WriteLine("  Attempting to fetch user...");
                await Task.Delay(10);
                return Result<User>.Fail("User not found");
            })
            .TapAsync(u => System.Console.WriteLine($"  This won't execute"))
            .MapAsync(async u =>
            {
                System.Console.WriteLine("  This won't execute either");
                await Task.Delay(10);
                return new UserDto();
            });

        System.Console.WriteLine($"Failed pipeline result: {failedResult.IsFailed}");
        if (failedResult.IsFailed)
        {
            System.Console.WriteLine($"  Error: {failedResult.Errors[0].Message}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Helper Methods (Async)

    private static async Task<Result<User>> GetUserAsync(int id)
    {
        await Task.Delay(10); // Simulate async operation
        return Result<User>.Ok(new User
        {
            Id = id,
            Name = $"User{id}",
            Email = $"user{id}@example.com",
            Age = 25
        });
    }

    private static async Task<Result<User>> ValidateUserAsync(User user)
    {
        await Task.Delay(10);
        return Result<User>.Ok(user, "User validated");
    }

    private static async Task<Result<User>> SaveUserAsync(User user)
    {
        await Task.Delay(10);
        return Result<User>.Ok(user, "User saved");
    }

    private static async Task<bool> IsEmailUniqueAsync(string email)
    {
        await Task.Delay(10);
        return true; // Simulate unique email check
    }

    private static async Task<bool> IsUserActiveAsync(int userId)
    {
        await Task.Delay(10);
        return true; // Simulate active check
    }

    #endregion

    #region Helper Classes

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public int Age { get; set; }
    }

    private class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Status { get; set; } = "";
    }

    #endregion
}
