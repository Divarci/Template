using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;

namespace Core.Services.System.AuditLogs;

internal sealed partial class AuditLogService
{
    public async Task<PaginatedResult<TDto[]>> GetPaginatedAsync<TDto>(
        Guid? entityId,
        int page,
        int pageSize,
        string sortBy,
        bool isAscending,
        string? keyword,
        DateTime? fromDate,
        DateTime? toDate,
        Func<AuditLog[], TDto[]> mapper,
        CancellationToken cancellationToken = default) where TDto : class
    {
        Result<AuditLog[]> entityResult = await _auditLogRepository
            .GetPaginatedAsync(
                entityId, page, pageSize,
                sortBy, isAscending, keyword, fromDate, toDate, cancellationToken);

        if (entityResult.IsFailure)
            return PaginatedResult<TDto[]>.Failure(
                message: entityResult.Message,
                statusCode: entityResult.StatusCode);

        Result<int> totalCountResult = await _auditLogRepository
            .CountByEntityAsync(entityId, keyword, fromDate, toDate, cancellationToken);

        if (totalCountResult.IsFailure)
            return PaginatedResult<TDto[]>.Failure(
                message: totalCountResult.Message,
                statusCode: totalCountResult.StatusCode);

        return PaginatedResult<TDto[]>.Success(
            message: "List retrieved successfully",
            data: mapper(entityResult.Data ?? []),
            pageNumber: page,
            pageSize: pageSize,
            totalCount: totalCountResult.Data,
            pageCount: (int)Math.Ceiling((double)totalCountResult.Data / pageSize));
    }
}
