using Core.Library.ResultPattern;

namespace Core.Library.Contracts.Caching;

/// <summary>
/// Service for managing distributed cache providing operations.
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Gets a single entity with caching support.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to return</typeparam>
    /// <param name="serviceMethod">The service method to execute</param>
    /// <param name="useCache">Whether to use caching</param>
    /// <param name="cacheKey">The cache key to use</param>
    /// <param name="cacheExpiration">Cache expiration time</param>
    /// <returns>Result containing the entity</returns>
    Task<Result<TEntity>> GetSingleAsync<TEntity>(
        Func<Task<Result<TEntity>>> serviceMethod,
        bool useCache,
        string cacheKey,
        TimeSpan cacheExpiration) where TEntity : class;

    /// <summary>
    /// Gets a paginated collection of items with caching
    /// </summary>
    /// <typeparam name="TDto">The item type returned by the service</typeparam>
    /// <param name="cacheKey">The cache key to use</param>
    /// <param name="useCache">Whether to use caching</param>
    /// <param name="serviceCall">The service call to execute</param>
    /// <param name="cacheExpiration">Cache expiration time</param>
    /// <returns>Paginated result containing the collection</returns>
    Task<PaginatedResult<TDto[]>> GetPaginatedCollectionAsync<TDto>(
        string cacheKey,
        bool useCache,
        Func<Task<PaginatedResult<TDto[]>>> serviceCall,
        TimeSpan cacheExpiration) where TDto : class;
}
