// using REslava.Result;

// namespace REslava.Result.Samples.Console.Examples;

// public static class RealWorldScenarios
// {
//     public static async Task Run()
//     {
//         System.Console.WriteLine("\\n1. User Registration Flow");
//         await DemoUserRegistration();

//         System.Console.WriteLine("\\n2. File Processing Pipeline");
//         await DemoFileProcessing();

//         System.Console.WriteLine("\\n3. API Call with Retry");
//         await DemoApiCallWithRetry();
//     }

//     private static async Task DemoUserRegistration()
//     {
//         var registration = new UserRegistration
//         {
//             Email = "john@example.com",
//             Password = "SecurePass123!",
//             Age = 25,
//             AcceptedTerms = true
//         };

//         var result = await RegisterUserAsync(registration);

//         result.Match(
//             onSuccess: user => System.Console.WriteLine($"✓ User registered: {user.Email}"),
//             onFailure: errors => System.Console.WriteLine($"✗ Registration failed: {string.Join(", ", errors.Select(e => e.Message))}")
//         );
//     }

//     private static async Task<Result<User>> RegisterUserAsync(UserRegistration request)
//     {
//         return await (await Result<string>.Ok(request.Email)
//             .EnsureNotNull("Email is required")
//             .Ensure(e => e.Contains("@"), "Invalid email format")
//             .EnsureAsync(async e => !await EmailExistsAsync(e), "Email already registered"))  // ← This returns Task<Result<string>>
//             .BindAsync(async email =>  // ← Now we can call BindAsync on Result<string>
//                 await Result<string>.Ok(request.Password)
//                     .Ensure(
//                         (p => p.Length >= 8, new Error("Min 8 characters")),
//                         (p => p.Any(char.IsDigit), new Error("Requires digit")),
//                         (p => p.Any(char.IsUpper), new Error("Requires uppercase"))
//                     )
//                     .Ensure(p => request.Age >= 18, "Must be 18 or older")
//                     .Ensure(p => request.AcceptedTerms, "Must accept terms")
//                     .MapAsync(async password => new User
//                     {
//                         Email = email,
//                         PasswordHash = await HashPasswordAsync(password),
//                         CreatedAt = DateTime.UtcNow
//                     }))
//             .TapAsync(async user => await SaveUserAsync(user))
//             .TapAsync(async user => await SendWelcomeEmailAsync(user.Email));
//     }

//     //private static async Task DemoFileProcessing()
//     //{
//     //    var filePath = "sample-data.txt";

//     //    var result = await Result<string>.Try(
//     //            () => filePath,
//     //            ex => new Error("File not found").WithTags("Path", filePath)
//     //        )
//     //        .Bind(path => Result<string>.Ok("Line 1\nLine 2\nLine 3")// await ReadFileAsync(path))
//     //        .Map(content => content.Split('\n'))
//     //        .Ensure(lines => lines.Length > 0, "File is empty")
//     //        .Map(lines => lines.Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)))
//     //        .Map(lines => new { Count = lines.Count(), Data = lines });

//     //    result.Match(
//     //        onSuccess: data => System.Console.WriteLine($"✓ Processed {data.Count} lines"),
//     //        onFailure: errors => System.Console.WriteLine($"✗ Processing failed: {errors[0].Message}")
//     //    );
//     //}
//     private static async Task DemoFileProcessing()
//     {
//         var filePath = "sample-data.txt";

//         var result = await Result<string>.Try(
//                 () => filePath,
//                 ex => new Error("File not found").WithTags("Path", filePath)
//             )
//             .BindAsync(async path => await ReadFileAsync(path));

//         // Now work with the Result_string directly
//         var finalResult = result
//             .Map(content => content.Split('\n'))
//             .Ensure(lines => lines.Length > 0, "File is empty")
//             .Map(lines => lines.Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)))
//             .Map(lines => new { Count = lines.Count(), Data = lines });

//         finalResult.Match(
//             onSuccess: data => System.Console.WriteLine($"✓ Processed {data.Count} lines"),
//             onFailure: errors => System.Console.WriteLine($"✗ Processing failed: {errors[0].Message}")
//         );
//     }

//     private static async Task DemoFileProcessingVer2()
//     {
//         var filePath = "sample-data.txt";

//         var result = await Result<string>.Try(
//                 () => filePath,
//                 ex => new Error("File not found").WithTags("Path", filePath)
//             )
//             .BindAsync(async path => await ReadFileAsync(path))
//             .Map(content => content.Split('\n'))
//             .Ensure(lines => lines.Length > 0, "File is empty")
//             .Map(lines => lines.Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)))
//             .Map(lines => new { Count = lines.Count(), Data = lines });

//         result.Match(
//             onSuccess: data => System.Console.WriteLine($"✓ Processed {data.Count} lines"),
//             onFailure: errors => System.Console.WriteLine($"✗ Processing failed: {errors[0].Message}")
//         );
//     }

//     private static async Task DemoApiCallWithRetry()
//     {
//         var userId = 123;
//         var maxRetries = 3;

//         var result = await RetryAsync(
//             async () => await FetchUserFromApiAsync(userId),
//             maxRetries,
//             TimeSpan.FromSeconds(1)
//         );

//         result.Match(
//             onSuccess: user => System.Console.WriteLine($"✓ Fetched user: {user.Name}"),
//             onFailure: errors => System.Console.WriteLine($"✗ Failed after {maxRetries} retries: {errors[0].Message}")
//         );
//     }

//     // Helper methods (simulated)
//     private static Task<bool> EmailExistsAsync(string email) =>
//         Task.FromResult(false);

//     private static Task<string> HashPasswordAsync(string password) =>
//         Task.FromResult($"hashed_{password}");

//     private static Task SaveUserAsync(User user) =>
//         Task.CompletedTask;

//     private static Task SendWelcomeEmailAsync(string email) =>
//         Task.CompletedTask;

//     private static Task<Result<string>> ReadFileAsync(string path) =>
//         Task.FromResult(Result<string>.Ok("Line 1\nLine 2\nLine 3"));

//     private static Task<Result<User>> FetchUserFromApiAsync(int userId) =>
//         Task.FromResult(Result<User>.Ok(new User { Id = userId, Name = "John" }));

//     private static async Task<Result<T>> RetryAsync<T>(
//         Func<Task<Result<T>>> operation,
//         int maxRetries,
//         TimeSpan delay)
//     {
//         for (int i = 0; i < maxRetries; i++)
//         {
//             var result = await operation();
//             if (result.IsSuccess)
//                 return result;

//             if (i < maxRetries - 1)
//                 await Task.Delay(delay);
//         }

//         return await operation();
//     }
// }

// public class UserRegistration
// {
//     public string Email { get; set; } = "";
//     public string Password { get; set; } = "";
//     public int Age { get; set; }
//     public bool AcceptedTerms { get; set; }
// }

// public class User
// {
//     public int Id { get; set; }
//     public string Name { get; set; } = "";
//     public string Email { get; set; } = "";
//     public string PasswordHash { get; set; } = "";
//     public DateTime CreatedAt { get; set; }
// }
