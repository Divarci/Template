using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;

namespace Core.Services.System.OutboxMessages;

internal sealed partial class OutboxMessageService
{
    public async Task<Result<OutboxMessage>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _outboxMessageRepository
            .GetByIdAsync(id, cancellationToken);
    }
}
