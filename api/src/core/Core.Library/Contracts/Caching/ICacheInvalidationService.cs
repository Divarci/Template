namespace Core.Library.Contracts.Caching;

/// <summary>
/// Service for managing distributed cache invalidation operations.
/// </summary>
public interface ICacheInvalidationService
{
    /// <summary>
    /// Invalidates a single entity cache entry using the exact cache key.
    /// </summary>
    /// <param name="cacheKey">The cache key</param>
    Task InvalidateEntity(string cacheKey);

    /// <summary>
    /// Invalidates ALL collection caches for the entire entity using pattern matching.
    /// Clears collections across all services due to potential cross-service data relationships.
    /// Matches the format: {entityName}_collection_*
    /// </summary>
    Task InvalidateCollections();
}
