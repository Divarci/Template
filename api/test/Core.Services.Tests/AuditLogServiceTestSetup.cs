using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Services.System.AuditLogs;
using Moq;

namespace Core.Services.Tests;

internal sealed class AuditLogServiceTestSetup
{
    public AuditLogServiceTestSetup()
    {
        Service = new AuditLogService(AuditLogRepository.Object);
    }

    public Mock<IAuditLogRepository> AuditLogRepository { get; } = new();

    public AuditLogService Service { get; }
}
