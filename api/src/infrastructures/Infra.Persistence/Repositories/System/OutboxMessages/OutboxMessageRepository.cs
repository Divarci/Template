using Core.Domain.Entities.System.OutboxMessages;
using Core.Domain.Entities.System.OutboxMessages.Interfaces;
using Infra.Persistence.Context;
using Infra.Persistence.Repositories.Generics;

namespace Infra.Persistence.Repositories.System.OutboxMessages;

internal sealed partial class OutboxMessageRepository(
    AppDbContext context) : Repository<OutboxMessage>(context), IOutboxMessageRepository
{
}
