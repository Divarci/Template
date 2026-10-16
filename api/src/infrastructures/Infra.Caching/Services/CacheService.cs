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

    public async Task InvalidateEntityAsync(string cacheKey)
    {
        try
        {
            await _db.KeyDeleteAsync(new RedisKey(cacheKey));

            _logger.LogDebug("Cached data removed with cache key: '{CacheKey}'.", cacheKey);
        }
        catch (Exception ex)
        {
            string traceId = Guid.NewGuid().ToString();

            _logger.LogWarning(ex, "Cache invalidation failed for key '{CacheKey}'. TraceId: {TraceId}", cacheKey, traceId);
        }
    }

    public async Task InvalidateCollectionsAsync()
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
        catch (Exception ex)
        {
            string traceId = Guid.NewGuid().ToString();

            _logger.LogWarning(ex, "Cache collection invalidation failed. TraceId: {TraceId}", traceId);
        }
    }

    public async Task<Result<TDto>> GetSingleAsync<TDto>(
        Func<Task<Result<TDto>>> serviceMethod,
        bool useCache,
        string cacheKey,
        TimeSpan cacheExpiration) where TDto : class
    {
        if (useCache && await GetAsync<TDto>(cacheKey) is { } cachedModel)
        {
            _logger.LogDebug("Cache hit for key '{CacheKey}'.", cacheKey);

            return Result<TDto>.Success(
                message: "Retrieved from cache",
                data: cachedModel,
                statusCode: HttpStatusCode.OK);
        }

        _logger.LogDebug("Cache miss for key '{CacheKey}'. Fetching from service.", cacheKey);

        Result<TDto> result = await serviceMethod();

        if (result.IsFailureAndNoData)
            return Result<TDto>.Failure(
                message: result.Message,
                statusCode: result.StatusCode);

        await SetAsync(cacheKey, result.Data, cacheExpiration);

        return Result<TDto>.Success(
            message: result.Message,
            data: result.Data,
            statusCode: result.StatusCode);
    }

    public async Task<PaginatedResult<TDto[]>> GetPaginatedCollectionAsync<TDto>(
        string cacheKey,
        bool useCache,
        Func<Task<PaginatedResult<TDto[]>>> serviceCall,
        TimeSpan cacheExpiration) where TDto : class
    {
        if (useCache && await GetAsync<PaginatedResult<TDto[]>>(cacheKey) is { Data: not null, Metadata: not null } cachedData)
        {
            _logger.LogDebug("Cache hit for key '{CacheKey}'.", cacheKey);

            return PaginatedResult<TDto[]>.Success(
                message: "List retrieved from cache",
                data: cachedData.Data,
                pageNumber: cachedData.Metadata.PageNumber,
                pageSize: cachedData.Metadata.PageSize,
                totalCount: cachedData.Metadata.TotalCount,
                pageCount: cachedData.Metadata.PageCount);
        }

        _logger.LogDebug("Cache miss for key '{CacheKey}'. Fetching from service.", cacheKey);

        PaginatedResult<TDto[]> dataResult = await serviceCall();

        if (dataResult.IsFailureAndNoData)
            return dataResult;

        await SetAsync(cacheKey, dataResult, cacheExpiration);

        return dataResult;
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
        catch (Exception ex)
        {
            string traceId = Guid.NewGuid().ToString();

            _logger.LogWarning(ex, "Cache read failed for key '{Key}'. TraceId: {TraceId}", key, traceId);

            return default;
        }
    }

    private async Task SetAsync<T>(string key, T value, TimeSpan slidingExpiration)
    {
        if (slidingExpiration <= TimeSpan.Zero || string.IsNullOrWhiteSpace(key))
        {
            _logger.LogDebug("Cache write skipped for key '{Key}': the key is empty or the expiration is not positive.", key);

            return;
        }

        try
        {
            string serialisedData = JsonSerializer.Serialize(value, _jsonOptions);

            await _db.StringSetAsync(key, serialisedData, slidingExpiration);

            _logger.LogDebug("Cache data stored for key '{Key}'.", key);
        }
        catch (Exception ex)
        {
            string traceId = Guid.NewGuid().ToString();

            _logger.LogWarning(ex, "Cache write failed for key '{Key}'. TraceId: {TraceId}", key, traceId);
        }
    }
}
