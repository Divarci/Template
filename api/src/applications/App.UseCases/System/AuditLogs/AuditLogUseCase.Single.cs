using Core.Domain.Entities.System.AuditLogs;
using Core.Library.Helpers;
using Core.Library.ResultPattern;

namespace App.UseCases.System.AuditLogs;

internal sealed partial class AuditLogUseCase
{
    public async Task<Result<TDto>> GetByIdAsync<TDto>(
        Guid id,
        Func<AuditLog, TDto> mapper,
        CancellationToken cancellationToken = default) where TDto : class
    {
        string cacheKey = CacheKeyHelper.Single(nameof(AuditLog), ("id", id));

        Result<TDto> auditLogResult = await _cacheProvider.GetSingleAsync(
            serviceMethod: async () => await _auditLogService.GetByIdAsync(
                id, mapper, cancellationToken),
            useCache: true,
            cacheKey: cacheKey,
            _timeout);

        return auditLogResult;
    }
}
