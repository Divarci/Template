using Core.Domain.Entities.System.AuditLogs;

namespace Adapter.Restful.Tests;

internal static class AuditLogTestData
{
    public static readonly DateTime CreatedOnUtc = new(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    public static AuditLog CreateAuditLog(
        Guid? entityId = null,
        string description = "Note created successfully",
        DateTime? createdOnUtc = null) =>
        AuditLog.Create(
            entityId ?? Guid.NewGuid(),
            description,
            createdOnUtc ?? CreatedOnUtc).Data!;
}
