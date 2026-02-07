using System;
using System.Collections.Generic;

namespace REslava.Result.AdvancedPatterns
{
    /// <summary>
    /// Represents a value that can be one of four possible types.
    /// A type-safe discriminated union for functional programming patterns.
    /// </summary>
    /// <typeparam name="T1">The first possible type.</typeparam>
    /// <typeparam name="T2">The second possible type.</typeparam>
    /// <typeparam name="T3">The third possible type.</typeparam>
    /// <typeparam name="T4">The fourth possible type.</typeparam>
    /// <remarks>
    /// <para>
    /// OneOf&lt;T1, T2, T3, T4&gt; provides a type-safe way to represent a value that can be one of four types.
    /// This is useful for scenarios where you need to handle different types of values or states
    /// without using null references, exceptions, or complex enums.
    /// </para>
    /// <para>
    /// Common use cases include:
    /// - API responses with four states: Success, ValidationError, NotFoundError, ServerError
    /// - Configuration values: String, Integer, Boolean, Double
    /// - Database operations: Created, Updated, Deleted, Conflict
    /// - File operations: Read, Write, Delete, Archive results
    /// </para>
    /// <example>
    /// <code>
    /// // API response handling
    /// OneOf&lt;SuccessData, ValidationError, NotFoundError, ServerError&gt; response = await CallApi();
    /// return response.Match(
    ///     case1: data => ProcessData(data),
    ///     case2: validationError => HandleValidationError(validationError),
    ///     case3: notFoundError => HandleNotFoundError(notFoundError),
    ///     case4: serverError => HandleServerError(serverError)
    /// );
    /// 
    /// // Configuration parsing
    /// OneOf&lt;string, int, bool, double&gt; config = ParseConfigValue("timeout");
    /// var processed = config.Match(
    ///     case1: text => $"String: {text.ToUpper()}",
    ///     case2: number => $"Number: {number * 2}",
    ///     case3: flag => $"Boolean: {flag}",
    ///     case4: decimal => $"Double: {decimal:F2}"
    /// );
    /// </code>
    /// </example>
    /// </remarks>
    public readonly struct OneOf<T1, T2, T3, T4> : IEquatable<OneOf<T1, T2, T3, T4>>
    {
        private readonly T1 _value1;
        private readonly T2 _value2;
        private readonly T3 _value3;
        private readonly T4 _value4;
        private readonly byte _index;

        /// <summary>
        /// Gets whether the OneOf contains a value of type T1.
        /// </summary>
        public bool IsT1 => _index == 0;

        /// <summary>
        /// Gets whether the OneOf contains a value of type T2.
        /// </summary>
        public bool IsT2 => _index == 1;

        /// <summary>
        /// Gets whether the OneOf contains a value of type T3.
        /// </summary>
        public bool IsT3 => _index == 2;

        /// <summary>
        /// Gets whether the OneOf contains a value of type T4.
        /// </summary>
        public bool IsT4 => _index == 3;

        /// <summary>
        /// Gets the value as T1 if it contains T1, otherwise throws InvalidOperationException.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the OneOf contains T2, T3, or T4.</exception>
        public T1 AsT1 => _index == 0 ? _value1 : throw new InvalidOperationException("OneOf contains T2, T3, or T4, not T1");

        /// <summary>
        /// Gets the value as T2 if it contains T2, otherwise throws InvalidOperationException.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the OneOf contains T1, T3, or T4.</exception>
        public T2 AsT2 => _index == 1 ? _value2 : throw new InvalidOperationException("OneOf contains T1, T3, or T4, not T2");

        /// <summary>
        /// Gets the value as T3 if it contains T3, otherwise throws InvalidOperationException.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the OneOf contains T1, T2, or T4.</exception>
        public T3 AsT3 => _index == 2 ? _value3 : throw new InvalidOperationException("OneOf contains T1, T2, or T4, not T3");

        /// <summary>
        /// Gets the value as T4 if it contains T4, otherwise throws InvalidOperationException.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the OneOf contains T1, T2, or T3.</exception>
        public T4 AsT4 => _index == 3 ? _value4 : throw new InvalidOperationException("OneOf contains T1, T2, or T3, not T4");

        /// <summary>
        /// Private constructor for internal use.
        /// </summary>
        private OneOf(T1 value1, T2 value2, T3 value3, T4 value4, byte index)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _index = index;
        }

        /// <summary>
        /// Creates a OneOf containing a T1 value.
        /// </summary>
        /// <param name="value">The T1 value to wrap.</param>
        /// <returns>A OneOf containing the specified T1 value.</returns>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, ValidationError, NotFoundError, Success&gt; result = OneOf&lt;Error, ValidationError, NotFoundError, Success&gt;.FromT1(new NotFoundError());
        /// </code>
        /// </example>
        public static OneOf<T1, T2, T3, T4> FromT1(T1 value)
        {
            return new OneOf<T1, T2, T3, T4>(value, default!, default!, default!, 0);
        }

        /// <summary>
        /// Creates a OneOf containing a T2 value.
        /// </summary>
        /// <param name="value">The T2 value to wrap.</param>
        /// <returns>A OneOf containing the specified T2 value.</returns>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, ValidationError, NotFoundError, Success&gt; result = OneOf&lt;Error, ValidationError, NotFoundError, Success&gt;.FromT2(new ValidationError("Invalid input"));
        /// </code>
        /// </example>
        public static OneOf<T1, T2, T3, T4> FromT2(T2 value)
        {
            return new OneOf<T1, T2, T3, T4>(default!, value, default!, default!, 1);
        }

        /// <summary>
        /// Creates a OneOf containing a T3 value.
        /// </summary>
        /// <param name="value">The T3 value to wrap.</param>
        /// <returns>A OneOf containing the specified T3 value.</returns>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, ValidationError, NotFoundError, Success&gt; result = OneOf&lt;Error, ValidationError, NotFoundError, Success&gt;.FromT3(new NotFoundError("Resource not found"));
        /// </code>
        /// </example>
        public static OneOf<T1, T2, T3, T4> FromT3(T3 value)
        {
            return new OneOf<T1, T2, T3, T4>(default!, default!, value, default!, 2);
        }

        /// <summary>
        /// Creates a OneOf containing a T4 value.
        /// </summary>
        /// <param name="value">The T4 value to wrap.</param>
        /// <returns>A OneOf containing the specified T4 value.</returns>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, ValidationError, NotFoundError, Success&gt; result = OneOf&lt;Error, ValidationError, NotFoundError, Success&gt;.FromT4(new Success("Operation completed"));
        /// </code>
        /// </example>
        public static OneOf<T1, T2, T3, T4> FromT4(T4 value)
        {
            return new OneOf<T1, T2, T3, T4>(default!, default!, default!, value, 3);
        }

        /// <summary>
        /// Implicit conversion from T1 to OneOf&lt;T1, T2, T3, T4&gt;.
        /// </summary>
        /// <param name="value">The T1 value to convert.</param>
        /// <returns>A OneOf containing the T1 value.</returns>
        public static implicit operator OneOf<T1, T2, T3, T4>(T1 value)
        {
            return FromT1(value);
        }

        /// <summary>
        /// Implicit conversion from T2 to OneOf&lt;T1, T2, T3, T4&gt;.
        /// </summary>
        /// <param name="value">The T2 value to convert.</param>
        /// <returns>A OneOf containing the T2 value.</returns>
        public static implicit operator OneOf<T1, T2, T3, T4>(T2 value)
        {
            return FromT2(value);
        }

        /// <summary>
        /// Implicit conversion from T3 to OneOf&lt;T1, T2, T3, T4&gt;.
        /// </summary>
        /// <param name="value">The T3 value to convert.</param>
        /// <returns>A OneOf containing the T3 value.</returns>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, ValidationError, NotFoundError, Success&gt; result = new NotFoundError("Resource not found"); // Implicit conversion
        /// </code>
        /// </example>
        public static implicit operator OneOf<T1, T2, T3, T4>(T3 value)
        {
            return FromT3(value);
        }

        /// <summary>
        /// Implicit conversion from T4 to OneOf&lt;T1, T2, T3, T4&gt;.
        /// </summary>
        /// <param name="value">The T4 value to convert.</param>
        /// <returns>A OneOf containing the T4 value.</returns>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, ValidationError, NotFoundError, Success&gt; result = new Success("Operation completed"); // Implicit conversion
        /// </code>
        /// </example>
        public static implicit operator OneOf<T1, T2, T3, T4>(T4 value)
        {
            return FromT4(value);
        }

        /// <summary>
        /// Pattern matching - executes the appropriate function based on the contained type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="case1">The function to execute when the OneOf contains T1.</param>
        /// <param name="case2">The function to execute when the OneOf contains T2.</param>
        /// <param name="case3">The function to execute when the OneOf contains T3.</param>
        /// <param name="case4">The function to execute when the OneOf contains T4.</param>
        /// <returns>The result of the executed function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any case function is null.</exception>
        /// <remarks>
        /// Match provides a type-safe way to handle all four possible cases without casting.
        /// This is similar to pattern matching in functional languages.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, ValidationError, NotFoundError, Success&gt; result = ProcessData();
        /// string message = result.Match(
        ///     case1: error => $"Error: {error.Message}",
        ///     case2: validationError => $"Validation Error: {validationError.Message}",
        ///     case3: notFoundError => $"Not Found: {notFoundError.Message}",
        ///     case4: success => $"Success: {success.Message}"
        /// );
        /// </code>
        /// </example>
        public TResult Match<TResult>(Func<T1, TResult> case1, Func<T2, TResult> case2, Func<T3, TResult> case3, Func<T4, TResult> case4)
        {
            if (case1 == null) throw new ArgumentNullException(nameof(case1));
            if (case2 == null) throw new ArgumentNullException(nameof(case2));
            if (case3 == null) throw new ArgumentNullException(nameof(case3));
            if (case4 == null) throw new ArgumentNullException(nameof(case4));

            return _index switch
            {
                0 => case1(_value1),
                1 => case2(_value2),
                2 => case3(_value3),
                3 => case4(_value4),
                _ => throw new InvalidOperationException("Invalid OneOf state")
            };
        }

        /// <summary>
        /// Executes an action based on the contained type.
        /// </summary>
        /// <param name="case1">The action to execute when the OneOf contains T1.</param>
        /// <param name="case2">The action to execute when the OneOf contains T2.</param>
        /// <param name="case3">The action to execute when the OneOf contains T3.</param>
        /// <param name="case4">The action to execute when the OneOf contains T4.</param>
        /// <exception cref="ArgumentNullException">Thrown when any case action is null.</exception>
        /// <remarks>
        /// Switch is useful for side effects when you don't need to return a value.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, ValidationError, NotFoundError, Success&gt; result = ProcessData();
        /// result.Switch(
        ///     case1: error => Console.WriteLine($"Error: {error.Message}"),
        ///     case2: validationError => Console.WriteLine($"Validation Error: {validationError.Message}"),
        ///     case3: notFoundError => Console.WriteLine($"Not Found: {notFoundError.Message}"),
        ///     case4: success => Console.WriteLine($"Success: {success.Message}")
        /// );
        /// </code>
        /// </example>
        public void Switch(Action<T1> case1, Action<T2> case2, Action<T3> case3, Action<T4> case4)
        {
            if (case1 == null) throw new ArgumentNullException(nameof(case1));
            if (case2 == null) throw new ArgumentNullException(nameof(case2));
            if (case3 == null) throw new ArgumentNullException(nameof(case3));
            if (case4 == null) throw new ArgumentNullException(nameof(case4));

            switch (_index)
            {
                case 0:
                    case1(_value1);
                    break;
                case 1:
                    case2(_value2);
                    break;
                case 2:
                    case3(_value3);
                    break;
                case 3:
                    case4(_value4);
                    break;
                default:
                    throw new InvalidOperationException("Invalid OneOf state");
            }
        }

        /// <summary>
        /// Maps the T2 value if present, otherwise propagates other types.
        /// </summary>
        /// <typeparam name="TNewT2">The new T2 type.</typeparam>
        /// <param name="mapper">The function to apply to the T2 value.</param>
        /// <returns>A new OneOf with the mapped T2 value or the original T1/T3.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        /// <remarks>
        /// MapT2 allows you to transform the T2 value without unwrapping the OneOf.
        /// If the OneOf contains T1 or T3, the mapper function is not called and those values are propagated.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, Success, Warning&gt; result = ProcessData();
        /// OneOf&lt;Error, ProcessedSuccess, Warning&gt; processed = result.MapT2(s => s.WithTimestamp());
        /// </code>
        /// </example>
        public OneOf<T1, TNewT2, T3> MapT2<TNewT2>(Func<T2, TNewT2> mapper)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            return _index switch
            {
                0 => OneOf<T1, TNewT2, T3>.FromT1(_value1),
                1 => OneOf<T1, TNewT2, T3>.FromT2(mapper(_value2)),
                2 => OneOf<T1, TNewT2, T3>.FromT3(_value3),
                _ => throw new InvalidOperationException("Invalid OneOf state")
            };
        }

        /// <summary>
        /// Maps the T3 value if present, otherwise propagates other types.
        /// </summary>
        /// <typeparam name="TNewT3">The new T3 type.</typeparam>
        /// <param name="mapper">The function to apply to the T3 value.</param>
        /// <returns>A new OneOf with the mapped T3 value or the original T1/T2.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        /// <remarks>
        /// MapT3 allows you to transform the T3 value without unwrapping the OneOf.
        /// If the OneOf contains T1 or T2, the mapper function is not called and those values are propagated.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, Success, Warning&gt; result = ProcessData();
        /// OneOf&lt;Error, Success, ProcessedWarning&gt; processed = result.MapT3(w => w.WithSeverity("High"));
        /// </code>
        /// </example>
        public OneOf<T1, T2, TNewT3> MapT3<TNewT3>(Func<T3, TNewT3> mapper)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            return _index switch
            {
                0 => OneOf<T1, T2, TNewT3>.FromT1(_value1),
                1 => OneOf<T1, T2, TNewT3>.FromT2(_value2),
                2 => OneOf<T1, T2, TNewT3>.FromT3(mapper(_value3)),
                _ => throw new InvalidOperationException("Invalid OneOf state")
            };
        }

        /// <summary>
        /// Binds the T2 value if present, otherwise propagates other types.
        /// </summary>
        /// <typeparam name="TNewT2">The new T2 type.</typeparam>
        /// <param name="binder">The function that takes T2 and returns a OneOf.</param>
        /// <returns>The result of the binder function or the original T1/T3.</returns>
        /// <exception cref="ArgumentNullException">Thrown when binder is null.</exception>
        /// <remarks>
        /// BindT2 (also known as flatMap or chain) allows you to chain operations that return OneOf.
        /// This is useful for sequential operations where each step might return a different type.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, Success, Warning&gt; result = GetInitialResult();
        /// OneOf&lt;Error, ProcessedData, Warning&gt; final = result.BindT2(success => ProcessSuccess(success));
        /// </code>
        /// </example>
        public OneOf<T1, TNewT2, T3> BindT2<TNewT2>(Func<T2, OneOf<T1, TNewT2, T3>> binder)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));

            return _index switch
            {
                0 => OneOf<T1, TNewT2, T3>.FromT1(_value1),
                1 => binder(_value2),
                2 => OneOf<T1, TNewT2, T3>.FromT3(_value3),
                _ => throw new InvalidOperationException("Invalid OneOf state")
            };
        }

        /// <summary>
        /// Binds the T3 value if present, otherwise propagates other types.
        /// </summary>
        /// <typeparam name="TNewT3">The new T3 type.</typeparam>
        /// <param name="binder">The function that takes T3 and returns a OneOf.</param>
        /// <returns>The result of the binder function or the original T1/T2.</returns>
        /// <exception cref="ArgumentNullException">Thrown when binder is null.</exception>
        /// <remarks>
        /// BindT3 (also known as flatMap or chain) allows you to chain operations that return OneOf.
        /// This is useful for sequential operations where each step might return a different type.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, Success, Warning&gt; result = GetInitialResult();
        /// OneOf&lt;Error, Success, ProcessedWarning&gt; final = result.BindT3(warning => HandleWarning(warning));
        /// </code>
        /// </example>
        public OneOf<T1, T2, TNewT3> BindT3<TNewT3>(Func<T3, OneOf<T1, T2, TNewT3>> binder)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));

            return _index switch
            {
                0 => OneOf<T1, T2, TNewT3>.FromT1(_value1),
                1 => OneOf<T1, T2, TNewT3>.FromT2(_value2),
                2 => binder(_value3),
                _ => throw new InvalidOperationException("Invalid OneOf state")
            };
        }

        /// <summary>
        /// Converts to string for debugging.
        /// </summary>
        /// <returns>A string representation of the OneOf.</returns>
        /// <remarks>
        /// Returns the string representation of the contained value with type information.
        /// This is primarily useful for debugging and logging.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, Success, Warning&gt; result = new Success("Data processed");
        /// Console.WriteLine(result.ToString()); // "OneOf&lt;T1, T2, T3&gt;(T2: Success { Message = Data processed })"
        /// </code>
        /// </example>
        public override string ToString()
        {
            return _index switch
            {
                0 => $"OneOf<{typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}>(T1: {_value1})",
                1 => $"OneOf<{typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}>(T2: {_value2})",
                2 => $"OneOf<{typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}>(T3: {_value3})",
                _ => "OneOf<Invalid>"
            };
        }

        /// <summary>
        /// Indicates whether the current OneOf is equal to another OneOf of the same type.
        /// </summary>
        /// <param name="other">The other OneOf to compare with.</param>
        /// <returns>true if the OneOf instances are equal; otherwise, false.</returns>
        public bool Equals(OneOf<T1, T2, T3, T4> other)
        {
            if (_index != other._index) return false;

            return _index switch
            {
                0 => EqualityComparer<T1>.Default.Equals(_value1, other._value1),
                1 => EqualityComparer<T2>.Default.Equals(_value2, other._value2),
                2 => EqualityComparer<T3>.Default.Equals(_value3, other._value3),
                3 => EqualityComparer<T4>.Default.Equals(_value4, other._value4),
                _ => false
            };
        }

        /// <summary>
        /// Indicates whether the current OneOf is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is OneOf<T1, T2, T3, T4> other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for the OneOf.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(_index, _value1, _value2, _value3, _value4);
        }

        /// <summary>
        /// Determines whether two OneOf instances are equal.
        /// </summary>
        /// <param name="left">The left OneOf to compare.</param>
        /// <param name="right">The right OneOf to compare.</param>
        /// <returns>true if the OneOf instances are equal; otherwise, false.</returns>
        public static bool operator ==(OneOf<T1, T2, T3, T4> left, OneOf<T1, T2, T3, T4> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two OneOf instances are not equal.
        /// </summary>
        /// <param name="left">The left OneOf to compare.</param>
        /// <param name="right">The right OneOf to compare.</param>
        /// <returns>true if the OneOf instances are not equal; otherwise, false.</returns>
        public static bool operator !=(OneOf<T1, T2, T3, T4> left, OneOf<T1, T2, T3, T4> right)
        {
            return !left.Equals(right);
        }
    }
}
