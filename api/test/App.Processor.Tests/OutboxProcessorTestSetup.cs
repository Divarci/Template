using App.Processor.DomainEvents;
using App.Processor.DomainEvents.Samples.Notes;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Domain.Entities.System.OutboxMessages.Interfaces;
using Core.Library.Contracts.UnitOfWorks;
using Core.Library.ResultPattern;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace App.Processor.Tests;

internal sealed class OutboxProcessorTestSetup
{
    private readonly Mock<ITransactionalUnitOfWork> _handlerUnitOfWork = new();
    private readonly Mock<IServiceScope> _serviceScope = new();
    private readonly Mock<IServiceProvider> _serviceProvider = new();
    private readonly Mock<ILogger<NoteCreatedDomainEventHandler>> _handlerLogger = new();

    public OutboxProcessorTestSetup()
    {
        UnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<Result>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<Task<Result>> operation, CancellationToken _) => operation());

        _handlerUnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<Result>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<Task<Result>> operation, CancellationToken _) => operation());

        ServiceScopeFactory
            .Setup(f => f.CreateScope())
            .Returns(_serviceScope.Object);

        _serviceScope
            .Setup(s => s.ServiceProvider)
            .Returns(_serviceProvider.Object);

        _serviceProvider
            .Setup(p => p.GetService(typeof(NoteCreatedDomainEventHandler)))
            .Returns(() => new NoteCreatedDomainEventHandler(
                _handlerUnitOfWork.Object,
                NoteService.Object,
                _handlerLogger.Object));

        Processor = new OutboxProcessor(
            UnitOfWork.Object,
            ServiceScopeFactory.Object,
            OutboxMessageService.Object,
            Logger.Object);
    }

    public Mock<ITransactionalUnitOfWork> UnitOfWork { get; } = new();

    public Mock<IServiceScopeFactory> ServiceScopeFactory { get; } = new();

    public Mock<IOutboxMessageService> OutboxMessageService { get; } = new();

    public Mock<INoteService> NoteService { get; } = new();

    public Mock<ILogger<OutboxProcessor>> Logger { get; } = new();

    public OutboxProcessor Processor { get; }

    public void GivenNoteFound(Note note)
    {
        NoteService
            .Setup(s => s.GetByIdAsync(
                note.Id,
                It.IsAny<Func<Note, Note>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note>.Success(
                "Note retrieved",
                note,
                HttpStatusCode.OK));
    }

    public void GivenPendingMessages(params OutboxMessage[] messages)
    {
        OutboxMessageService
            .Setup(s => s.GetUnprocessedMessagesAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<OutboxMessage>>.Success(
                "Messages retrieved",
                messages,
                HttpStatusCode.OK));

        foreach (OutboxMessage message in messages)
            OutboxMessageService
                .Setup(s => s.GetByIdAsync(
                    message.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<OutboxMessage>.Success(
                    "Message retrieved",
                    message,
                    HttpStatusCode.OK));
    }

    public void GivenHandlerThrows(Exception exception)
    {
        _handlerUnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<Result>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }
}
