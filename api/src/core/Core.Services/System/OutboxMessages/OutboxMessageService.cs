using Core.Domain.Entities.System.OutboxMessages.Interfaces;

namespace Core.Services.System.OutboxMessages;

internal sealed partial class OutboxMessageService(
    IOutboxMessageRepository outboxMessageRepository) : IOutboxMessageService
{
    private readonly IOutboxMessageRepository _outboxMessageRepository = outboxMessageRepository;
}
