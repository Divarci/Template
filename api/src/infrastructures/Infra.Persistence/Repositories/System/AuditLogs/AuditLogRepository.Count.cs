using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Infra.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infra.Persistence.Repositories.System.AuditLogs;

internal sealed partial class AuditLogRepository
{
    public async Task<Result<int>> CountFilteredAsync(
        Guid? entityId,
        string? keyword,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        int count = await _context.Set<AuditLog>()
            .AsNoTracking()
            .WhereIf(entityId.HasValue, x => x.EntityId == entityId!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                EF.Functions.ILike(x.Description, $"%{keyword}%"))
            .WhereIf(fromDate.HasValue, x => x.CreatedOnUtc >= fromDate!.Value)
            .WhereIf(toDate.HasValue, x => x.CreatedOnUtc <= toDate!.Value)
            .CountAsync(cancellationToken);

        return Result<int>.Success(
            message: "Audit log count retrieved successfully.",
            data: count,
            statusCode: HttpStatusCode.OK);
    }
}
