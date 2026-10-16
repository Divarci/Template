using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Library.Contracts.Caching;

namespace App.UseCases.System.AuditLogs;

internal sealed partial class AuditLogUseCase(
    IAuditLogService auditLogService,
    ICacheProvider cacheProvider) : IAuditLogUseCase
{
    private readonly IAuditLogService _auditLogService = auditLogService;
    private readonly ICacheProvider _cacheProvider = cacheProvider;

    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(30);
}
