using Adapter.Restful.Controllers.VersionOne.System.AuditLogs.Models.Responses;
using Core.Domain.Entities.System.AuditLogs;

namespace Adapter.Restful.Controllers.VersionOne.System.AuditLogs;

internal static class AuditLogMapper
{
    public static AuditLogResponse ToResponse(AuditLog auditLog)
    {
        return new AuditLogResponse
        {
            Id = auditLog.Id,
            EntityId = auditLog.EntityId,
            Description = auditLog.Description,
            CreatedOnUtc = auditLog.CreatedOnUtc,
        };
    }

    public static AuditLogResponse[] ToArrayResponse(AuditLog[] auditLogs)
    {
        return [.. auditLogs.Select(ToResponse)];
    }
}
