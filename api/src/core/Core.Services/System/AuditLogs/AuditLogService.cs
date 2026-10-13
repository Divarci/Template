using Core.Domain.Entities.System.AuditLogs.Interfaces;

namespace Core.Services.System.AuditLogs;

internal sealed partial class AuditLogService(
    IAuditLogRepository auditLogRepository) : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository = auditLogRepository;
}
