using Core.Domain.Entities.System.AuditLogs;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Infra.Persistence.Context;
using Infra.Persistence.Repositories.Generics;

namespace Infra.Persistence.Repositories.System.AuditLogs;

internal sealed partial class AuditLogRepository(
    AppDbContext context) : Repository<AuditLog>(context), IAuditLogRepository
{
}
