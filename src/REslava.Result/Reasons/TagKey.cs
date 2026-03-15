namespace REslava.Result;

/// <summary>
/// Base type for typed tag keys that provide type-safe access to <see cref="IReason.Tags"/>.
/// </summary>
public abstract record TagKey(string Name);

/// <summary>
/// A typed key for accessing a tag value of type <typeparamref name="T"/> from <see cref="IReason.Tags"/>.
/// </summary>
/// <typeparam name="T">The expected type of the tag value.</typeparam>
/// <example>
/// <code>
/// // Define a custom key
/// var myKey = new TagKey&lt;string&gt;("MyKey");
///
/// // Or use predefined keys
/// error.TryGet(DomainTags.Entity, out var entity);
/// </code>
/// </example>
public sealed record TagKey<T>(string Name) : TagKey(Name);
