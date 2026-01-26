# Reasons API Reference

This document contains all public methods and properties available in the Reason types.

## Reason.Generic

Base class providing fluent API for all reason types with CRTP pattern.

| Method | Signature | Description |
|--------|-----------|-------------|
| **WithMessage** | `TReason WithMessage(string message)` | Creates a new instance with updated message (immutable) |
| **WithTag** | `TReason WithTag(string key, object value)` | Creates a new instance with an additional tag (immutable). Throws if key already exists |
| **WithTags** | `TReason WithTags(params (string key, object value)[] tags)` | Creates a new instance with multiple additional tags (immutable). Throws if any key already exists |
| **WithTagsFrom** | `TReason WithTagsFrom(ImmutableDictionary<string, object> tags)` | Creates a new instance with additional tags from a dictionary (immutable). Throws if any key already exists |

## Success

Represents a successful operation outcome.

| Constructor | Signature | Description |
|-------------|-----------|-------------|
| **Success** | `Success(string message)` | Creates a success reason with a specific message |

## Error

Represents an error operation outcome.

| Constructor | Signature | Description |
|-------------|-----------|-------------|
| **Error** | `Error(string message)` | Creates an error reason with a specific message |

## ExceptionError

Represents an error that wraps an exception with automatic tag generation.

| Constructor | Signature | Description |
|-------------|-----------|-------------|
| **ExceptionError** | `ExceptionError(Exception exception)` | Creates an exception error using exception message as error message |
| **ExceptionError** | `ExceptionError(string message, Exception exception)` | Creates an exception error with custom message and exception |

| Property | Type | Description |
|----------|------|-------------|
| **Exception** | `Exception` | Gets the wrapped exception |

## ConversionError

Represents an error that occurred during implicit type conversion.

| Constructor | Signature | Description |
|-------------|-----------|-------------|
| **ConversionError** | `ConversionError(string reason)` | Creates a conversion error with specified reason. Automatically tags as Warning severity |

| Method | Signature | Description |
|--------|-----------|-------------|
| **WithConversionType** | `ConversionError WithConversionType(string conversionType)` | Adds conversion-specific context to error |
| **WithProvidedValue** | `ConversionError WithProvidedValue(object? value)` | Adds provided value to error context |

## Interfaces

### IReason

Base interface for all reasons.

| Property | Type | Description |
|----------|------|-------------|
| **Message** | `string` | Gets the message describing the reason |
| **Tags** | `ImmutableDictionary<string, object>` | Gets the tags associated with the reason |

### ISuccess : IReason

Interface for success reasons.

### IError : IReason

Interface for error reasons.
