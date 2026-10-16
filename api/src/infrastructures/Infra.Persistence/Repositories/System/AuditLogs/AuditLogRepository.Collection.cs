using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Infra.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infra.Persistence.Repositories.System.AuditLogs;

internal sealed partial class AuditLogRepository
{
    public async Task<Result<AuditLog[]>> GetPaginatedAsync(
        Guid? entityId,
        int page,
        int pageSize,
        string sortBy,
        bool isAscending,
        string? keyword,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AuditLog> query = _context.Set<AuditLog>()
            .AsNoTracking()
            .WhereIf(entityId.HasValue, x => x.EntityId == entityId!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                EF.Functions.ILike(x.Description, $"%{keyword}%"))
            .WhereIf(fromDate.HasValue, x => x.CreatedOnUtc >= fromDate!.Value)
            .WhereIf(toDate.HasValue, x => x.CreatedOnUtc <= toDate!.Value);

        bool sortByEntityId = string.Equals(sortBy, "entityId", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<AuditLog> orderedQuery = sortByEntityId
            ? (isAscending
                ? query.OrderBy(x => x.EntityId)
                : query.OrderByDescending(x => x.EntityId))
            : (isAscending
                ? query.OrderBy(x => x.CreatedOnUtc)
                : query.OrderByDescending(x => x.CreatedOnUtc));

        AuditLog[] logs = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return Result<AuditLog[]>.Success(
            message: "Audit logs retrieved successfully.",
            data: logs,
            statusCode: HttpStatusCode.OK);
    }
}
