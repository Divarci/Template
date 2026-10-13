using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Microsoft.Extensions.Logging;

namespace Core.Services.Samples.Notes;

internal sealed partial class NoteService
{
    private async Task WriteAuditLogAsync(
        Guid entityId,
        string description,
        CancellationToken cancellationToken)
    {
        Result<AuditLog> auditLogResult = AuditLog.Create(entityId, description, DateTime.UtcNow);

        if (auditLogResult.IsFailureAndNoData)
        {
            _logger.LogWarning(
                "Failed to create audit log for Note {EntityId}. {Message} | TraceId: {TraceId}",
                entityId,
                auditLogResult.Message,
                auditLogResult.ErrorDetails?.TraceId ?? Guid.NewGuid().ToString());

            return;
        }

        Result<AuditLog> persistResult = await _auditLogRepository
            .CreateAsync(auditLogResult.Data, cancellationToken);

        if (persistResult.IsFailure)
            _logger.LogWarning(
                "Failed to stage audit log for Note {EntityId}. {Message} | TraceId: {TraceId}",
                entityId,
                persistResult.Message,
                Guid.NewGuid().ToString());
    }
}
