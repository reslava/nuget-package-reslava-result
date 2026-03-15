using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Internal helper that injects <see cref="ResultContext"/> fields into error tags — non-overwriting.
/// Only fields present in the context but absent from the error's existing tags are injected.
/// </summary>
internal static class ResultContextEnricher
{
    /// <summary>
    /// Returns a new error with context fields injected as tags (non-overwriting).
    /// If no fields need to be added, the original error instance is returned unchanged.
    /// </summary>
    internal static IError EnrichError(IError error, ResultContext? context)
    {
        if (context is null) return error;

        var tagsToAdd = BuildContextTags(error.Tags, context);
        if (tagsToAdd.Count == 0) return error;

        try
        {
            // All concrete errors extend Reason<TSelf> which has a public WithTagsFrom method.
            // dynamic dispatch resolves the correct CRTP override and preserves the concrete type.
            return (IError)((dynamic)error).WithTagsFrom(tagsToAdd);
        }
        catch
        {
            // Enrichment is best-effort — never break the pipeline over a tag injection failure.
            return error;
        }
    }

    /// <summary>
    /// Returns a new reasons list with context fields injected into each error reason.
    /// If no errors needed enrichment, the original list instance is returned unchanged.
    /// </summary>
    internal static ImmutableList<IReason> EnrichReasons(
        ImmutableList<IReason> reasons, ResultContext? context)
    {
        if (context is null) return reasons;

        bool anyChanged = false;
        var builder = ImmutableList.CreateBuilder<IReason>();

        foreach (var reason in reasons)
        {
            if (reason is IError error)
            {
                var enriched = EnrichError(error, context);
                builder.Add(enriched);
                if (!ReferenceEquals(enriched, error)) anyChanged = true;
            }
            else
            {
                builder.Add(reason);
            }
        }

        return anyChanged ? builder.ToImmutable() : reasons;
    }

    private static ImmutableDictionary<string, object> BuildContextTags(
        ImmutableDictionary<string, object> existingTags, ResultContext context)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, object>();

        void AddIfMissing(string key, string? value)
        {
            if (value is not null && !existingTags.ContainsKey(key))
                builder.Add(key, value);
        }

        AddIfMissing(DomainTags.Entity.Name, context.Entity);
        AddIfMissing(DomainTags.EntityId.Name, context.EntityId);
        AddIfMissing(DomainTags.CorrelationId.Name, context.CorrelationId);
        AddIfMissing(DomainTags.OperationName.Name, context.OperationName);
        AddIfMissing(DomainTags.TenantId.Name, context.TenantId);

        return builder.ToImmutable();
    }
}
