using Core.Domain.Entities.System.AuditLogs;
using Core.Library.Helpers;
using Core.Library.ResultPattern;

namespace App.UseCases.System.AuditLogs;

internal sealed partial class AuditLogUseCase
{
    public async Task<PaginatedResult<TDto[]>> GetPaginatedAsync<TDto>(
        Guid? entityId,
        int? pageQuery,
        int? pageSizeQuery,
        string? sortByQuery,
        bool? isAscendingQuery,
        string? keyword,
        DateTime? fromDate,
        DateTime? toDate,
        Func<AuditLog[], TDto[]> mapper,
        CancellationToken cancellationToken = default) where TDto : class
    {
        int page = pageQuery < 1 || pageQuery is null ? 1 : pageQuery.Value;
        int size = pageSizeQuery < 1 || pageSizeQuery is null ? 20 : pageSizeQuery.Value;
        string sortBy = string.IsNullOrWhiteSpace(sortByQuery) ? "createdDate" : sortByQuery;
        bool ascending = isAscendingQuery ?? false;

        string cacheKey = CacheKeyHelper.Collection(
            nameof(AuditLog),
            nameof(GetPaginatedAsync),
            [entityId?.ToString() ?? string.Empty, page, size, sortBy, ascending,
             keyword ?? string.Empty,
             fromDate?.ToString("O") ?? string.Empty, toDate?.ToString("O") ?? string.Empty]);

        PaginatedResult<TDto[]> retrievalResult = await _cacheProvider.GetPaginatedCollection(
            cacheKey: cacheKey,
            useCache: true,
            serviceCall: async () => await _auditLogService.GetPaginatedAsync(
                entityId, page, size, sortBy, ascending,
                keyword, fromDate, toDate, mapper, cancellationToken),
            _timeout);

        return retrievalResult;
    }
}
