using Core.Library.Contracts.Caching;
using Core.Library.ResultPattern;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Net;
using System.Text.Json;

namespace Infra.Caching.Services;

internal sealed class CacheService(
    IConnectionMultiplexer redis,
    ILogger<CacheService> logger) : ICacheProvider, ICacheInvalidationService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly IServer _server = redis.GetServer(redis.GetEndPoints().First());
    private readonly ILogger<CacheService> _logger = logger;
    private readonly JsonSerializerOptions _jsonOptions = SerializerOptions.Instance;

    public async Task InvalidateEntity(string cacheKey)
    {
        try
        {
            await _db.KeyDeleteAsync(new RedisKey(cacheKey));

            _logger.LogDebug("Cached data removed with cache key: '{CacheKey}'.", cacheKey);
        }
        catch (RedisException ex)
        {
            string traceId = Guid.NewGuid().ToString();

            _logger.LogWarning(ex, "Cache invalidation failed for key '{CacheKey}'. TraceId: {TraceId}", cacheKey, traceId);
        }
    }

    public async Task InvalidateCollections()
    {
        try
        {
            RedisKey[] keys = await _server
                .KeysAsync(pattern: "collection_*")
                .ToArrayAsync();

            if (keys.Length > 0)
                await _db.KeyDeleteAsync(keys);

            _logger.LogDebug("Cached data collections removed.");
        }
        catch (RedisException ex)
        {
            string traceId = Guid.NewGuid().ToString();

            _logger.LogWarning(ex, "Cache collection invalidation failed. TraceId: {TraceId}", traceId);
        }
    }

    public async Task<Result<TEntity>> GetSingleAsync<TEntity>(
        Func<Task<Result<TEntity>>> serviceMethod,
        bool useCache,
        string cacheKey,
        TimeSpan cacheExpiration) where TEntity : class
    {
        if (useCache && await GetAsync<TEntity>(cacheKey) is { } cachedModel)
        {
            _logger.LogDebug("Cache hit for key '{CacheKey}'.", cacheKey);

            return Result<TEntity>.Success(
                message: "Retrieved from cache",
                data: cachedModel,
                statusCode: HttpStatusCode.OK);
        }

        _logger.LogDebug("Cache miss for key '{CacheKey}'. Fetching from service.", cacheKey);

        Result<TEntity> result = await serviceMethod();

        if (result.IsFailureAndNoData)
            return Result<TEntity>.Failure(
                message: result.Message,
                statusCode: result.StatusCode);

        TEntity entity = result.Data;

        await SetAsync(cacheKey, entity, cacheExpiration);

        return Result<TEntity>.Success(
            message: result.Message,
            data: entity,
            statusCode: result.StatusCode);
    }

    public async Task<PaginatedResult<TDto[]>> GetPaginatedCollection<TDto>(
        string cacheKey,
        bool useCache,
        Func<Task<PaginatedResult<TDto[]>>> serviceCall,
        TimeSpan cacheExpiration) where TDto : class
    {
        if (useCache && await GetAsync<PaginatedResult<TDto[]>>(cacheKey) is { } cachedData)
        {
            _logger.LogDebug("Cache hit for key '{CacheKey}'.", cacheKey);

            return PaginatedResult<TDto[]>.Success(
                message: "List retrieved from cache",
                data: cachedData.Data ?? [],
                pageNumber: cachedData.Metadata?.PageNumber ?? 0,
                pageSize: cachedData.Metadata?.PageSize ?? 0,
                totalCount: cachedData.Metadata?.TotalCount ?? 0,
                pageCount: cachedData.Metadata?.PageCount ?? 0);
        }

        _logger.LogDebug("Cache miss for key '{CacheKey}'. Fetching from service.", cacheKey);

        PaginatedResult<TDto[]> dataResult = await serviceCall();

        if (dataResult.IsFailureAndNoData)
            return dataResult;

        if (cacheExpiration.TotalSeconds > 0 && !string.IsNullOrWhiteSpace(cacheKey))
            await SetAsync(cacheKey, dataResult, cacheExpiration);

        return PaginatedResult<TDto[]>.Success(
            message: dataResult.Message,
            data: dataResult.Data ?? [],
            pageNumber: dataResult.Metadata?.PageNumber ?? 0,
            pageSize: dataResult.Metadata?.PageSize ?? 0,
            totalCount: dataResult.Metadata?.TotalCount ?? 0,
            pageCount: dataResult.Metadata?.PageCount ?? 0);
    }

    private async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            string? cachedData = await _db.StringGetAsync(key);

            if (cachedData is null)
                return default;

            return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
        }
        catch (RedisException ex)
        {
            string traceId = Guid.NewGuid().ToString();

            _logger.LogWarning(ex, "Cache read failed for key '{Key}'. TraceId: {TraceId}", key, traceId);

            return default;
        }
    }

    private async Task SetAsync<T>(string key, T value, TimeSpan slidingExpiration)
    {
        try
        {
            string serialisedData = JsonSerializer.Serialize(value, _jsonOptions);

            await _db.StringSetAsync(key, serialisedData, slidingExpiration);

            _logger.LogDebug("Cache data stored for key '{Key}'.", key);
        }
        catch (RedisException ex)
        {
            string traceId = Guid.NewGuid().ToString();

            _logger.LogWarning(ex, "Cache write failed for key '{Key}'. TraceId: {TraceId}", key, traceId);
        }
    }
}
