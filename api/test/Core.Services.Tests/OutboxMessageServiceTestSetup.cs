using Core.Domain.Entities.System.OutboxMessages.Interfaces;
using Core.Services.System.OutboxMessages;
using Moq;

namespace Core.Services.Tests;

internal sealed class OutboxMessageServiceTestSetup
{
    public OutboxMessageServiceTestSetup()
    {
        Service = new OutboxMessageService(OutboxMessageRepository.Object);
    }

    public Mock<IOutboxMessageRepository> OutboxMessageRepository { get; } = new();

    public OutboxMessageService Service { get; }
}
