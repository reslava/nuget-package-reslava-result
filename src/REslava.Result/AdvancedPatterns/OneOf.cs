using System;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.AdvancedPatterns
{
    /// <summary>
    /// Represents a value that can be one of two possible types.
    /// A type-safe discriminated union for functional programming patterns.
    /// </summary>
    /// <typeparam name="T1">The first possible type.</typeparam>
    /// <typeparam name="T2">The second possible type.</typeparam>
    /// <remarks>
    /// <para>
    /// OneOf&lt;T1, T2&gt; provides a type-safe way to represent a value that can be one of two types.
    /// This is useful for scenarios where you need to handle different types of values or errors
    /// without using null references or exceptions.
    /// </para>
    /// <para>
    /// Common use cases include:
    /// - Error handling with typed errors: OneOf&lt;Error, Success&gt;
    /// - Configuration values: OneOf&lt;string, int&gt;
    /// - API responses: OneOf&lt;ValidationError, Data&gt;
    /// </para>
    /// <example>
    /// <code>
    /// // Error handling
    /// OneOf&lt;Error, User&gt; user = GetUser(id);
    /// return user.Match(
    ///     error => HandleError(error),
    ///     user => ProcessUser(user)
    /// );
    /// 
    /// // Configuration parsing
    /// OneOf&lt;string, int&gt; config = GetConfig("timeout");
    /// int timeout = config.Match(
    ///     str => int.Parse(str),
    ///     num => num
    /// );
    /// </code>
    /// </example>
    /// </remarks>
    public readonly struct OneOf<T1, T2> : IEquatable<OneOf<T1, T2>>
    {
        private readonly T1 _value1;
        private readonly T2 _value2;
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
        /// Gets the value as T1 if it contains T1, otherwise throws InvalidOperationException.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the OneOf contains T2.</exception>
        public T1 AsT1 => _index == 0 ? _value1 : throw new InvalidOperationException("OneOf contains T2, not T1");

        /// <summary>
        /// Gets the value as T2 if it contains T2, otherwise throws InvalidOperationException.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the OneOf contains T1.</exception>
        public T2 AsT2 => _index == 1 ? _value2 : throw new InvalidOperationException("OneOf contains T1, not T2");

        /// <summary>
        /// Private constructor for internal use.
        /// </summary>
        private OneOf(T1 value1, T2 value2, byte index)
        {
            _value1 = value1;
            _value2 = value2;
            _index = index;
        }

        /// <summary>
        /// Creates a OneOf containing a T1 value.
        /// </summary>
        /// <param name="value">The T1 value to wrap.</param>
        /// <returns>A OneOf containing the specified T1 value.</returns>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, User&gt; result = OneOf&lt;Error, User&gt;.FromT1(new NotFoundError());
        /// </code>
        /// </example>
        public static OneOf<T1, T2> FromT1(T1 value)
        {
            return new OneOf<T1, T2>(value, default!, 0);
        }

        /// <summary>
        /// Creates a OneOf containing a T2 value.
        /// </summary>
        /// <param name="value">The T2 value to wrap.</param>
        /// <returns>A OneOf containing the specified T2 value.</returns>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, User&gt; result = OneOf&lt;Error, User&gt;.FromT2(new User("Alice"));
        /// </code>
        /// </example>
        public static OneOf<T1, T2> FromT2(T2 value)
        {
            return new OneOf<T1, T2>(default!, value, 1);
        }

        /// <summary>
        /// Implicit conversion from T1 to OneOf&lt;T1, T2&gt;.
        /// </summary>
        /// <param name="value">The T1 value to convert.</param>
        /// <returns>A OneOf containing the T1 value.</returns>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, User&gt; result = new NotFoundError(); // Implicit conversion
        /// </code>
        /// </example>
        public static implicit operator OneOf<T1, T2>(T1 value)
        {
            return FromT1(value);
        }

        /// <summary>
        /// Implicit conversion from T2 to OneOf&lt;T1, T2&gt;.
        /// </summary>
        /// <param name="value">The T2 value to convert.</param>
        /// <returns>A OneOf containing the T2 value.</returns>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, User&gt; result = new User("Alice"); // Implicit conversion
        /// </code>
        /// </example>
        public static implicit operator OneOf<T1, T2>(T2 value)
        {
            return FromT2(value);
        }

        /// <summary>
        /// Pattern matching - executes the appropriate function based on the contained type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="case1">The function to execute when the OneOf contains T1.</param>
        /// <param name="case2">The function to execute when the OneOf contains T2.</param>
        /// <returns>The result of the executed function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when case1 or case2 is null.</exception>
        /// <remarks>
        /// Match provides a type-safe way to handle both possible cases without casting.
        /// This is similar to pattern matching in functional languages.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, User&gt; result = GetUser(id);
        /// string message = result.Match(
        ///     error => $"Error: {error.Message}",
        ///     user => $"User: {user.Name}"
        /// );
        /// </code>
        /// </example>
        public TResult Match<TResult>(Func<T1, TResult> case1, Func<T2, TResult> case2)
        {
            if (case1 == null) throw new ArgumentNullException(nameof(case1));
            if (case2 == null) throw new ArgumentNullException(nameof(case2));

            return _index switch
            {
                0 => case1(_value1),
                1 => case2(_value2),
                _ => throw new InvalidOperationException("Invalid OneOf state")
            };
        }

        /// <summary>
        /// Executes an action based on the contained type.
        /// </summary>
        /// <param name="case1">The action to execute when the OneOf contains T1.</param>
        /// <param name="case2">The action to execute when the OneOf contains T2.</param>
        /// <exception cref="ArgumentNullException">Thrown when case1 or case2 is null.</exception>
        /// <remarks>
        /// Switch is useful for side effects when you don't need to return a value.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, User&gt; result = GetUser(id);
        /// result.Switch(
        ///     error => Console.WriteLine($"Error: {error.Message}"),
        ///     user => Console.WriteLine($"User: {user.Name}")
        /// );
        /// </code>
        /// </example>
        public void Switch(Action<T1> case1, Action<T2> case2)
        {
            if (case1 == null) throw new ArgumentNullException(nameof(case1));
            if (case2 == null) throw new ArgumentNullException(nameof(case2));

            switch (_index)
            {
                case 0:
                    case1(_value1);
                    break;
                case 1:
                    case2(_value2);
                    break;
                default:
                    throw new InvalidOperationException("Invalid OneOf state");
            }
        }

        /// <summary>
        /// Maps the T2 value if present, otherwise propagates T1.
        /// </summary>
        /// <typeparam name="TNewT2">The new T2 type.</typeparam>
        /// <param name="mapper">The function to apply to the T2 value.</param>
        /// <returns>A new OneOf with the mapped T2 value or the original T1.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        /// <remarks>
        /// Map allows you to transform the T2 value without unwrapping the OneOf.
        /// If the OneOf contains T1, the mapper function is not called and T1 is propagated.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, User&gt; user = GetUser(id);
        /// OneOf&lt;Error, string&gt; userName = user.Map(u => u.Name);
        /// </code>
        /// </example>
        public OneOf<T1, TNewT2> Map<TNewT2>(Func<T2, TNewT2> mapper)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            return _index switch
            {
                0 => OneOf<T1, TNewT2>.FromT1(_value1),
                1 => OneOf<T1, TNewT2>.FromT2(mapper(_value2)),
                _ => throw new InvalidOperationException("Invalid OneOf state")
            };
        }

        /// <summary>
        /// Binds the T2 value if present, otherwise propagates T1.
        /// </summary>
        /// <typeparam name="TNewT2">The new T2 type.</typeparam>
        /// <param name="binder">The function that takes T2 and returns a OneOf.</param>
        /// <returns>The result of the binder function or the original T1.</returns>
        /// <exception cref="ArgumentNullException">Thrown when binder is null.</exception>
        /// <remarks>
        /// Bind (also known as flatMap or chain) allows you to chain operations that return OneOf.
        /// This is useful for sequential operations where each step might return a different type.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, int&gt; userId = GetUserId();
        /// OneOf&lt;Error, User&gt; user = userId.Bind(id =&gt; GetUser(id));
        /// </code>
        /// </example>
        public OneOf<T1, TNewT2> Bind<TNewT2>(Func<T2, OneOf<T1, TNewT2>> binder)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));

            return _index switch
            {
                0 => OneOf<T1, TNewT2>.FromT1(_value1),
                1 => binder(_value2),
                _ => throw new InvalidOperationException("Invalid OneOf state")
            };
        }

        /// <summary>
        /// Filters the T2 value if it satisfies the predicate, otherwise converts to T1.
        /// </summary>
        /// <param name="predicate">The condition to test the T2 value against.</param>
        /// <param name="fallbackT1">The T1 value to use when the predicate fails.</param>
        /// <returns>The original OneOf if the predicate is true, otherwise the fallback T1.</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
        /// <remarks>
        /// Filter allows you to conditionally keep or discard the T2 value.
        /// If the OneOf contains T1, the predicate is not evaluated and T1 is returned.
        /// </remarks>
        /// <example>
        /// <code>
        /// OneOf&lt;Error, User&gt; user = GetUser(id);
        /// OneOf&lt;Error, User&gt; activeUser = user.Filter(u => u.IsActive, new UserInactiveError());
        /// </code>
        /// </example>
        public OneOf<T1, T2> Filter(Func<T2, bool> predicate, T1 fallbackT1)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return _index switch
            {
                0 => this, // Return original T1
                1 => predicate(_value2) ? this : OneOf<T1, T2>.FromT1(fallbackT1),
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
        /// OneOf&lt;Error, User&gt; user = new User("Alice");
        /// Console.WriteLine(user.ToString()); // "OneOf&lt;T1, T2&gt;(T2: User { Name = Alice })"
        /// </code>
        /// </example>
        public override string ToString()
        {
            return _index switch
            {
                0 => $"OneOf<{typeof(T1).Name}, {typeof(T2).Name}>(T1: {_value1})",
                1 => $"OneOf<{typeof(T1).Name}, {typeof(T2).Name}>(T2: {_value2})",
                _ => "OneOf<Invalid>"
            };
        }

        /// <summary>
        /// Determines equality between two OneOf instances.
        /// </summary>
        /// <param name="other">The other OneOf to compare with.</param>
        /// <returns>true if the OneOf instances are equal; otherwise, false.</returns>
        public bool Equals(OneOf<T1, T2> other)
        {
            if (_index != other._index) return false;

            return _index switch
            {
                0 => EqualityComparer<T1>.Default.Equals(_value1, other._value1),
                1 => EqualityComparer<T2>.Default.Equals(_value2, other._value2),
                _ => false
            };
        }

        /// <summary>
        /// Determines equality between the OneOf and another object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is OneOf<T1, T2> other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for the OneOf.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(_index, _value1, _value2);
        }

        /// <summary>
        /// Equality operator for OneOf instances.
        /// </summary>
        /// <param name="left">The left OneOf.</param>
        /// <param name="right">The right OneOf.</param>
        /// <returns>true if the OneOf instances are equal; otherwise, false.</returns>
        public static bool operator ==(OneOf<T1, T2> left, OneOf<T1, T2> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for OneOf instances.
        /// </summary>
        /// <param name="left">The left OneOf.</param>
        /// <param name="right">The right OneOf.</param>
        /// <returns>true if the OneOf instances are not equal; otherwise, false.</returns>
        public static bool operator !=(OneOf<T1, T2> left, OneOf<T1, T2> right)
        {
            return !left.Equals(right);
        }
    }
}
