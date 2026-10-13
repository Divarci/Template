using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;

namespace Core.Services.System.AuditLogs;

internal sealed partial class AuditLogService
{
    public async Task<Result<TDto>> GetByIdAsync<TDto>(
        Guid id,
        Func<AuditLog, TDto> mapper,
        CancellationToken cancellationToken = default) where TDto : class
    {
        Result<AuditLog> entityResult = await _auditLogRepository
            .GetByIdAsync(id, cancellationToken);

        if (entityResult.IsFailureAndNoData)
            return Result<TDto>.Failure(
                message: entityResult.Message,
                statusCode: entityResult.StatusCode);

        return Result<TDto>.Success(
            message: entityResult.Message,
            data: mapper(entityResult.Data),
            statusCode: entityResult.StatusCode);
    }
}
