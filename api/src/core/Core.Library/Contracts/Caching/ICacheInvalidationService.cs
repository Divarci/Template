namespace Core.Library.Contracts.Caching;

/// <summary>
/// Service for managing distributed cache invalidation operations.
/// </summary>
public interface ICacheInvalidationService
{
    /// <summary>
    /// Invalidates a single entity cache entry by deleting the exact cache key.
    /// The key is built with <c>CacheKeyHelper.Single</c> and has the form <c>{entity}:{label}({value})</c>.
    /// Invalidation is best-effort: a cache failure is logged and never thrown.
    /// </summary>
    /// <param name="cacheKey">The exact cache key of the entry to remove.</param>
    /// <returns>A task representing the asynchronous invalidate operation.</returns>
    Task InvalidateEntityAsync(string cacheKey);

    /// <summary>
    /// Invalidates every cached collection, for all entities, by deleting all keys that match the pattern <c>collection_*</c>.
    /// Collection keys are built with <c>CacheKeyHelper.Collection</c> and always start with <c>collection_</c>.
    /// Invalidation is best-effort: a cache failure is logged and never thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous invalidate operation.</returns>
    Task InvalidateCollectionsAsync();
}
