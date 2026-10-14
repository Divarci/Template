using Core.Domain.Entities.System.AuditLogs;

namespace App.UseCases.Tests;

internal static class AuditLogTestData
{
    public static readonly DateTime FromDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime ToDate = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Func<AuditLog, string> DescriptionMapper = auditLog => auditLog.Description;

    public static readonly Func<AuditLog[], string[]> DescriptionsMapper = logs => [.. logs.Select(l => l.Description)];
}
