using System;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.AdvancedPatterns
{
    /// <summary>
    /// Represents an optional value that may or may not exist.
    /// A functional programming alternative to null references.
    /// </summary>
    /// <typeparam name="T">The type of the optional value</typeparam>
    /// <remarks>
    /// <para>
    /// Maybe&lt;T&gt; is a type-safe way to handle optional values without using null references.
    /// It can be in one of two states: Some(T) when it contains a value, or None when it's empty.
    /// </para>
    /// <para>
    /// This eliminates the need for null checks and provides a functional approach
    /// to handling potentially missing values.
    /// </para>
    /// <example>
    /// <code>
    /// // Instead of:
    /// string? name = GetUser();
    /// if (name != null) {
    ///     Console.WriteLine(name.ToUpper());
    /// }
    /// 
    /// // Use Maybe:
    /// Maybe&lt;string&gt; name = GetUser();
    /// var upperName = name.Map(n => n.ToUpper());
    /// </code>
    /// </example>
    /// </remarks>
    public readonly struct Maybe<T> : IEquatable<Maybe<T>>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        /// <summary>
        /// Gets whether this Maybe has a value.
        /// </summary>
        /// <value>
        /// <c>true</c> if this Maybe contains a value; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Use this property to check if the Maybe contains a value before accessing it.
        /// This is safer than checking for null.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;string&gt; maybe = Maybe&lt;string&gt;.Some("hello");
        /// if (maybe.HasValue) {
        ///     Console.WriteLine("Has value");
        /// }
        /// </code>
        /// </example>
        public bool HasValue => _hasValue;

        /// <summary>
        /// Gets the value if it exists, throws if not.
        /// </summary>
        /// <value>The contained value.</value>
        /// <exception cref="InvalidOperationException">Thrown when the Maybe has no value.</exception>
        /// <remarks>
        /// Only access this property when you're certain the Maybe contains a value.
        /// Consider using <see cref="ValueOrDefault(T)"/> or <see cref="Match{TResult}(Func{T, TResult}, Func{TResult})"/> for safer access.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;int&gt; maybe = Maybe&lt;int&gt;.Some(42);
        /// int value = maybe.Value; // Returns 42
        /// 
        /// Maybe&lt;int&gt; empty = Maybe&lt;int&gt;.None;
        /// int value2 = empty.Value; // Throws InvalidOperationException
        /// </code>
        /// </example>
        public T Value => _hasValue ? _value : throw new InvalidOperationException("Maybe does not have a value");

        private Maybe(T value, bool hasValue)
        {
            _value = value;
            _hasValue = hasValue;
        }

        /// <summary>
        /// Creates a Maybe with a value (Some).
        /// </summary>
        /// <param name="value">The value to wrap in a Maybe.</param>
        /// <returns>A Maybe containing the specified value.</returns>
        /// <remarks>
        /// This is the factory method for creating a Maybe that contains a value.
        /// The value can be null, in which case it will still be wrapped in a Some.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;string&gt; name = Maybe&lt;string&gt;.Some("Alice");
        /// Maybe&lt;int&gt; age = Maybe&lt;int&gt;.Some(25);
        /// Maybe&lt;string?&gt; nullable = Maybe&lt;string?&gt;.Some(null);
        /// </code>
        /// </example>
        public static Maybe<T> Some(T value)
        {
            return new Maybe<T>(value, true);
        }

        /// <summary>
        /// Creates a Maybe without a value (None).
        /// </summary>
        /// <value>A Maybe representing the absence of a value.</value>
        /// <remarks>
        /// This is the singleton instance representing an empty Maybe.
        /// Use this when you want to represent the absence of a value.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;string&gt; empty = Maybe&lt;string&gt;.None;
        /// Maybe&lt;int&gt; noNumber = Maybe&lt;int&gt;.None;
        /// </code>
        /// </example>
        public static Maybe<T> None { get; } = new Maybe<T>(default!, false);

        public bool Equals(Maybe<T> other)
        {
            if (!_hasValue && !other._hasValue) return true;
            if (_hasValue != other._hasValue) return false;
            return EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object? obj)
        {
            return obj is Maybe<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_hasValue, _value);
        }

        public static bool operator ==(Maybe<T> left, Maybe<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Maybe<T> left, Maybe<T> right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Implicit conversion from T to Maybe&lt;T&gt;.
        /// </summary>
        /// <param name="value">The value to convert to Maybe.</param>
        /// <returns>A Maybe containing the specified value.</returns>
        /// <remarks>
        /// This allows automatic conversion from any value to Maybe, making the code more concise.
        /// Null values will be wrapped in Some(null), not converted to None.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;string&gt; name = "Alice"; // Implicit conversion
        /// Maybe&lt;int&gt; number = 42; // Implicit conversion
        /// 
        /// // These are equivalent to:
        /// Maybe&lt;string&gt; name2 = Maybe&lt;string&gt;.Some("Alice");
        /// Maybe&lt;int&gt; number2 = Maybe&lt;int&gt;.Some(42);
        /// </code>
        /// </example>
        public static implicit operator Maybe<T>(T value)
        {
            return Some(value);
        }

        /// <summary>
        /// Transforms the value if it exists.
        /// </summary>
        /// <typeparam name="TResult">The type of the transformed value.</typeparam>
        /// <param name="mapper">The function to apply to the value if it exists.</param>
        /// <returns>A new Maybe containing the transformed value, or None if the original was None.</returns>
        /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
        /// <remarks>
        /// Map allows you to transform the contained value without unwrapping the Maybe.
        /// If the Maybe is None, the mapper function is not called and None is returned.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;int&gt; number = Maybe&lt;int&gt;.Some(5);
        /// Maybe&lt;string&gt; text = number.Map(n => $"Number: {n}"); // Some("Number: 5")
        /// 
        /// Maybe&lt;int&gt; empty = Maybe&lt;int&gt;.None;
        /// Maybe&lt;string&gt; noText = empty.Map(n => $"Number: {n}"); // None
        /// </code>
        /// </example>
        public Maybe<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            
            return _hasValue 
                ? Maybe<TResult>.Some(mapper(_value)) 
                : Maybe<TResult>.None;
        }

        /// <summary>
        /// Chains operations that return Maybe.
        /// </summary>
        /// <typeparam name="TResult">The type of the result Maybe.</typeparam>
        /// <param name="binder">The function that takes the value and returns a Maybe.</param>
        /// <returns>The result of the binder function, or None if the original was None.</returns>
        /// <exception cref="ArgumentNullException">Thrown when binder is null.</exception>
        /// <remarks>
        /// Bind (also known as flatMap or chain) allows you to chain operations that return Maybe.
        /// This is useful for sequential operations where each step might fail.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;int&gt; userId = Maybe&lt;int&gt;.Some(123);
        /// Maybe&lt;string&gt; userName = userId.Bind(id =&gt; FindUserById(id));
        /// 
        /// Maybe&lt;int&gt; invalidId = Maybe&lt;int&gt;.None;
        /// Maybe&lt;string&gt; noUser = invalidId.Bind(id =&gt; FindUserById(id)); // None
        /// </code>
        /// </example>
        public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> binder)
        {
            if (binder == null) throw new ArgumentNullException(nameof(binder));
            
            return _hasValue 
                ? binder(_value) 
                : Maybe<TResult>.None;
        }

        /// <summary>
        /// Keeps the value only if it satisfies the predicate.
        /// </summary>
        /// <param name="predicate">The condition to test the value against.</param>
        /// <returns>The original Maybe if the predicate is true, otherwise None.</returns>
        /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
        /// <remarks>
        /// Filter allows you to conditionally keep or discard the value.
        /// If the Maybe is None, the predicate is not evaluated and None is returned.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;int&gt; number = Maybe&lt;int&gt;.Some(10);
        /// Maybe&lt;int&gt; positive = number.Filter(n => n > 0); // Some(10)
        /// Maybe&lt;int&gt; large = number.Filter(n => n > 100); // None
        /// 
        /// Maybe&lt;int&gt; empty = Maybe&lt;int&gt;.None;
        /// Maybe&lt;int&gt; stillEmpty = empty.Filter(n => n > 0); // None
        /// </code>
        /// </example>
        public Maybe<T> Filter(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            
            return _hasValue && predicate(_value) 
                ? this 
                : None;
        }

        /// <summary>
        /// Gets the value or returns a default.
        /// </summary>
        /// <param name="defaultValue">The default value to return when no value exists.</param>
        /// <returns>The contained value or the specified default.</returns>
        /// <remarks>
        /// This is a safe way to extract the value without throwing exceptions.
        /// It's equivalent to the null-coalescing operator (??) for Maybe.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;string&gt; name = Maybe&lt;string&gt;.Some("Alice");
        /// string result1 = name.ValueOrDefault("Default"); // "Alice"
        /// 
        /// Maybe&lt;string&gt; empty = Maybe&lt;string&gt;.None;
        /// string result2 = empty.ValueOrDefault("Default"); // "Default"
        /// </code>
        /// </example>
        public T ValueOrDefault(T defaultValue = default!)
        {
            return _hasValue ? _value : defaultValue;
        }

        /// <summary>
        /// Pattern matching - executes the appropriate function.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="some">The function to execute when the Maybe has a value.</param>
        /// <param name="none">The function to execute when the Maybe has no value.</param>
        /// <returns>The result of the executed function.</returns>
        /// <exception cref="ArgumentNullException">Thrown when some or none is null.</exception>
        /// <remarks>
        /// Match provides a way to handle both cases (Some and None) in a type-safe manner.
        /// This is similar to pattern matching in functional languages.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;int&gt; number = Maybe&lt;int&gt;.Some(42);
        /// string message = number.Match(
        ///     some: n => $"The number is {n}",
        ///     none: () => "No number available"
        /// ); // "The number is 42"
        /// 
        /// Maybe&lt;int&gt; empty = Maybe&lt;int&gt;.None;
        /// string message2 = empty.Match(
        ///     some: n => $"The number is {n}",
        ///     none: () => "No number available"
        /// ); // "No number available"
        /// </code>
        /// </example>
        public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
        {
            if (some == null) throw new ArgumentNullException(nameof(some));
            if (none == null) throw new ArgumentNullException(nameof(none));
            
            return _hasValue ? some(_value) : none();
        }

        /// <summary>
        /// Executes an action if the value exists.
        /// </summary>
        /// <param name="action">The action to execute with the contained value.</param>
        /// <returns>The original Maybe for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
        /// <remarks>
        /// Tap is useful for side effects like logging without breaking the Maybe chain.
        /// The Maybe is returned unchanged, allowing further chaining.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;string&gt; name = Maybe&lt;string&gt;.Some("Alice");
        /// var result = name
        ///     .Tap(n => Console.WriteLine($"Processing: {n}"))
        ///     .Map(n => n.ToUpper());
        /// // Output: "Processing: Alice"
        /// // result: Some("ALICE")
        /// </code>
        /// </example>
        public Maybe<T> Tap(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            
            if (_hasValue) action(_value);
            return this;
        }

        /// <summary>
        /// Executes an action if the value doesn't exist.
        /// </summary>
        /// <param name="action">The action to execute when the Maybe has no value.</param>
        /// <returns>The original Maybe for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
        /// <remarks>
        /// TapNone is useful for handling the None case with side effects like logging defaults.
        /// The Maybe is returned unchanged, allowing further chaining.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;string&gt; name = Maybe&lt;string&gt;.None;
        /// var result = name
        ///     .TapNone(() => Console.WriteLine("Using default name"))
        ///     .ValueOrDefault("Default");
        /// // Output: "Using default name"
        /// // result: "Default"
        /// </code>
        /// </example>
        public Maybe<T> TapNone(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            
            if (!_hasValue) action();
            return this;
        }

        /// <summary>
        /// Converts to string for debugging.
        /// </summary>
        /// <returns>A string representation of the Maybe.</returns>
        /// <remarks>
        /// Returns "Some(value)" when the Maybe contains a value, or "None" when empty.
        /// This is primarily useful for debugging and logging.
        /// </remarks>
        /// <example>
        /// <code>
        /// Maybe&lt;string&gt; name = Maybe&lt;string&gt;.Some("Alice");
        /// Console.WriteLine(name.ToString()); // "Some(Alice)"
        /// 
        /// Maybe&lt;int&gt; empty = Maybe&lt;int&gt;.None;
        /// Console.WriteLine(empty.ToString()); // "None"
        /// </code>
        /// </example>
        public override string ToString()
        {
            return _hasValue ? $"Some({_value})" : "None";
        }
    }
}
