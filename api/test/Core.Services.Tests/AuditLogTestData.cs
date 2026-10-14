using Core.Domain.Entities.System.AuditLogs;

namespace Core.Services.Tests;

internal static class AuditLogTestData
{
    public static readonly DateTime FromDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime ToDate = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    public static AuditLog CreateAuditLog(string description = "Note created successfully") =>
        AuditLog.Create(
            Guid.NewGuid(),
            description,
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)).Data!;
}
