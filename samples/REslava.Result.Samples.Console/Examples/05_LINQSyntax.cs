using REslava.Result;

namespace REslava.Result.Samples.Console;

/// <summary>
/// Demonstrates LINQ query syntax with the Result pattern.
/// </summary>
public static class LINQSyntaxSamples
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== LINQ Syntax Samples ===\n");

        SimpleSelect();
        SelectWithTransformation();
        WhereClause();
        SelectMany();
        ComplexQuery();
        await AsyncLINQ();
        MultipleFromClauses();
        await CompleteAsyncQuery();

        System.Console.WriteLine("\n=== LINQ Syntax Complete ===\n");
    }

    #region Simple Select

    private static void SimpleSelect()
    {
        System.Console.WriteLine("--- Simple Select ---");

        // Query syntax
        var result1 = from x in Result<int>.Ok(10)
                      select x * 2;

        System.Console.WriteLine($"Query syntax: {result1.Value}");

        // Method syntax (equivalent)
        var result2 = Result<int>.Ok(10)
            .Select(x => x * 2);

        System.Console.WriteLine($"Method syntax: {result2.Value}");

        // Failed result propagates
        var result3 = from x in Result<int>.Fail("Error")
                      select x * 2;

        System.Console.WriteLine($"Failed result: {result3.IsFailed}");

        System.Console.WriteLine();
    }

    #endregion

    #region Select with Transformation

    private static void SelectWithTransformation()
    {
        System.Console.WriteLine("--- Select with Transformation ---");

        // Transform user to DTO
        var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };

        var result = from u in Result<User>.Ok(user)
                     select new UserDto
                     {
                         Id = u.Id,
                         DisplayName = u.Name,
                         Email = u.Email
                     };

        System.Console.WriteLine($"User to DTO: {result.IsSuccess}");
        if (result.IsSuccess)
        {
            System.Console.WriteLine($"  DTO: {result.Value.DisplayName}");
        }

        // Chained transformations
        var result2 = from x in Result<int>.Ok(5)
                      select x * 2 into doubled
                      select doubled + 10 into added
                      select $"Result: {added}";

        System.Console.WriteLine($"Chained: {result2.Value}");

        System.Console.WriteLine();
    }

    #endregion

    #region Where Clause

    private static void WhereClause()
    {
        System.Console.WriteLine("--- Where Clause ---");

        // Simple where
        var result1 = from x in Result<int>.Ok(25)
                      where x > 18
                      select x;

        System.Console.WriteLine($"Where (pass): {result1.IsSuccess}, Value: {result1.Value}");

        // Failed where
        var result2 = from x in Result<int>.Ok(15)
                      where x > 18
                      select x;

        System.Console.WriteLine($"Where (fail): {result2.IsFailed}");
        if (result2.IsFailed)
        {
            System.Console.WriteLine($"  Error: {result2.Errors[0].Message}");
        }

        // Where with custom error message
        var result3 = Result<int>.Ok(15)
            .Where(x => x > 18, "Must be greater than 18");

        System.Console.WriteLine($"Where with message: {result3.IsFailed}");
        if (result3.IsFailed)
        {
            System.Console.WriteLine($"  Error: {result3.Errors[0].Message}");
        }

        // Multiple where clauses
        var result4 = from x in Result<int>.Ok(25)
                      where x > 0
                      where x < 100
                      where x % 5 == 0
                      select x;

        System.Console.WriteLine($"Multiple where: {result4.IsSuccess}, Value: {result4.Value}");

        System.Console.WriteLine();
    }

    #endregion

    #region SelectMany

    private static void SelectMany()
    {
        System.Console.WriteLine("--- SelectMany ---");

        // SelectMany (method syntax) - equivalent to Bind
        var result1 = Result<int>.Ok(5)
            .SelectMany(x => Result<string>.Ok($"Value: {x}"));

        System.Console.WriteLine($"SelectMany: {result1.Value}");

        // Failed SelectMany propagates
        var result2 = Result<int>.Ok(5)
            .SelectMany(x => Result<string>.Fail("Conversion failed"));

        System.Console.WriteLine($"SelectMany (failed): {result2.IsFailed}");

        // Chained SelectMany
        var result3 = Result<int>.Ok(1)
            .SelectMany(id => GetUser(id))
            .SelectMany(user => GetUserEmail(user));

        System.Console.WriteLine($"Chained SelectMany: {result3.Value}");

        System.Console.WriteLine();
    }

    #endregion

    #region Multiple From Clauses

    private static void MultipleFromClauses()
    {
        System.Console.WriteLine("--- Multiple From Clauses ---");

        // Two from clauses (uses SelectMany with resultSelector)
        var result1 = from x in Result<int>.Ok(5)
                      from y in Result<int>.Ok(10)
                      select x + y;

        System.Console.WriteLine($"Two from clauses: {result1.Value}");

        // Three from clauses
        var result2 = from x in Result<int>.Ok(2)
                      from y in Result<int>.Ok(3)
                      from z in Result<int>.Ok(4)
                      select x * y * z;

        System.Console.WriteLine($"Three from clauses: {result2.Value}");

        // With transformation
        var result3 = from userId in Result<int>.Ok(1)
                      from user in GetUser(userId)
                      from email in GetUserEmail(user)
                      select new { User = user.Name, Email = email };

        System.Console.WriteLine($"From with binding: {result3.Value.User}, {result3.Value.Email}");

        // Failed in chain
        var result4 = from x in Result<int>.Ok(5)
                      from y in Result<int>.Fail("Error in y")
                      from z in Result<int>.Ok(10)
                      select x + y + z;

        System.Console.WriteLine($"Failed from: {result4.IsFailed}");
        if (result4.IsFailed)
        {
            System.Console.WriteLine($"  Error: {result4.Errors[0].Message}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Complex Query

    private static void ComplexQuery()
    {
        System.Console.WriteLine("--- Complex Query ---");

        // Complete query with where, select, from
        var result = from userId in Result<int>.Ok(1)
                     where userId > 0
                     from user in GetUser(userId)
                     where user.Age >= 18
                     from userEmail in GetUserEmail(user)
                     where userEmail.Contains("@")
                     select new UserProfile
                     {
                         Id = userId,
                         Name = user.Name,
                         Email = userEmail,
                         IsAdult = user.Age >= 18
                     };

        System.Console.WriteLine($"Complex query: {result.IsSuccess}");
        if (result.IsSuccess)
        {
            var profile = result.Value;
            System.Console.WriteLine($"  Profile: {profile.Name} ({profile.Email})");
            System.Console.WriteLine($"  Adult: {profile.IsAdult}");
        }

        // Query with validation
        var email = "test@example.com";
        var validated = from e in Result<string>.Ok(email)
                        where !string.IsNullOrWhiteSpace(e)
                        where e.Contains("@")
                        where e.Length >= 5
                        where e.Length <= 100
                        select e.ToLower();

        System.Console.WriteLine($"\nEmail validation: {validated.IsSuccess}, Email: {validated.Value}");

        // Failed validation
        var invalid = from e in Result<string>.Ok("bad")
                      where !string.IsNullOrWhiteSpace(e)
                      where e.Contains("@")
                      select e;

        System.Console.WriteLine($"Invalid email: {invalid.IsFailed}");

        System.Console.WriteLine();
    }

    #endregion

    #region Async LINQ

    private static async Task AsyncLINQ()
    {
        System.Console.WriteLine("--- Async LINQ ---");

        // SelectAsync
        var result1 = await (from x in Result<int>.Ok(5)
                             select x)
            .SelectAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            });

        System.Console.WriteLine($"SelectAsync: {result1.Value}");

        // WhereAsync
        var result2 = await Result<int>.Ok(25)
            .WhereAsync(async x =>
            {
                await Task.Delay(10);
                return x > 18;
            }, "Must be greater than 18");

        System.Console.WriteLine($"WhereAsync: {result2.IsSuccess}");

        // SelectManyAsync
        var result3 = await Result<int>.Ok(1)
            .SelectManyAsync(async id =>
            {
                await Task.Delay(10);
                return await GetUserAsync(id);
            });

        System.Console.WriteLine($"SelectManyAsync: {result3.Value?.Name}");

        // Complex async query
        var result4 = await Result<int>.Ok(1)
            .SelectManyAsync(async id =>
            {
                await Task.Delay(10);
                return await GetUserAsync(id);
            })
            .WhereAsync(async user =>
            {
                await Task.Delay(10);
                return user.Age >= 18;
            }, "Must be adult")
            .SelectAsync(async user =>
            {
                await Task.Delay(10);
                return new { user.Name, user.Email };
            });

        System.Console.WriteLine($"Complex async: {result4.IsSuccess}");
        if (result4.IsSuccess)
        {
            System.Console.WriteLine($"  User: {result4.Value.Name}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Complete Async Query

    private static async Task CompleteAsyncQuery()
    {
        System.Console.WriteLine("--- Complete Async Query ---");

        // Comprehensive async pipeline using LINQ
        var userId = 1;

        var result = await (from id in Result<int>.Ok(userId)
                            select id)
            .WhereAsync(async id =>
            {
                await Task.Delay(10);
                return id > 0;
            }, "Invalid user ID")
            .SelectManyAsync(async id =>
            {
                System.Console.WriteLine($"  Fetching user {id}...");
                await Task.Delay(10);
                return await GetUserAsync(id);
            })
            .WhereAsync(async user =>
            {
                System.Console.WriteLine($"  Validating user {user.Name}...");
                await Task.Delay(10);
                return user.Age >= 18;
            }, "User must be 18+")
            .SelectAsync(async user =>
            {
                System.Console.WriteLine($"  Creating profile for {user.Name}...");
                await Task.Delay(10);
                return new UserProfile
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    IsAdult = user.Age >= 18
                };
            });

        System.Console.WriteLine($"Complete async query: {result.IsSuccess}");
        if (result.IsSuccess)
        {
            System.Console.WriteLine($"  Profile: {result.Value.Name} ({result.Value.Email})");
        }

        // Mixed sync/async query
        var mixed = await (from x in Result<int>.Ok(5)
                           where x > 0
                           select x)
            .SelectManyAsync(async x =>
            {
                await Task.Delay(10);
                return Result<string>.Ok($"Value: {x}");
            })
            .SelectAsync(async s =>
            {
                await Task.Delay(10);
                return s.ToUpper();
            });

        System.Console.WriteLine($"\nMixed sync/async: {mixed.Value}");

        System.Console.WriteLine();
    }

    #endregion

    #region Comparison: LINQ vs Fluent

    private static void ComparisonLINQvsFluent()
    {
        System.Console.WriteLine("--- LINQ vs Fluent Comparison ---");

        // LINQ query syntax
        var linq = from userId in Result<int>.Ok(1)
                   from user in GetUser(userId)
                   where user.Age >= 18
                   select user.Name;

        // Fluent method syntax (equivalent)
        var fluent = Result<int>.Ok(1)
            .Bind(userId => GetUser(userId))
            .Ensure(user => user.Age >= 18, "Must be 18+")
            .Map(user => user.Name);

        System.Console.WriteLine($"LINQ result: {linq.Value}");
        System.Console.WriteLine($"Fluent result: {fluent.Value}");
        System.Console.WriteLine($"Are equal: {linq.Value == fluent.Value}");

        System.Console.WriteLine();
    }

    #endregion

    #region Helper Methods

    private static Result<User> GetUser(int id)
    {
        return Result<User>.Ok(new User
        {
            Id = id,
            Name = $"User{id}",
            Email = $"user{id}@example.com",
            Age = 25
        });
    }

    private static Result<string> GetUserEmail(User user)
    {
        return Result<string>.Ok(user.Email);
    }

    private static async Task<Result<User>> GetUserAsync(int id)
    {
        await Task.Delay(10);
        return Result<User>.Ok(new User
        {
            Id = id,
            Name = $"User{id}",
            Email = $"user{id}@example.com",
            Age = 25
        });
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
        public string DisplayName { get; set; } = "";
        public string Email { get; set; } = "";
    }

    private class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsAdult { get; set; }
    }

    #endregion
}
