using App.Processor.DomainEvents.Samples.Notes;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Library.Contracts.UnitOfWorks;
using Core.Library.ResultPattern;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace App.Processor.Tests;

internal sealed class NoteCreatedDomainEventHandlerTestSetup
{
    public NoteCreatedDomainEventHandlerTestSetup()
    {
        UnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<Result>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<Result>> operation, CancellationToken _) =>
            {
                TransactionResult = await operation();
                return TransactionResult;
            });

        Handler = new NoteCreatedDomainEventHandler(
            UnitOfWork.Object,
            NoteService.Object,
            Logger.Object);
    }

    public Mock<ITransactionalUnitOfWork> UnitOfWork { get; } = new();

    public Mock<INoteService> NoteService { get; } = new();

    public Mock<ILogger<NoteCreatedDomainEventHandler>> Logger { get; } = new();

    public NoteCreatedDomainEventHandler Handler { get; }

    public Result? TransactionResult { get; private set; }

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

    public void GivenNoteMissing(
        Guid id,
        string message)
    {
        NoteService
            .Setup(s => s.GetByIdAsync(
                id,
                It.IsAny<Func<Note, Note>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note>.Failure(
                message,
                HttpStatusCode.NotFound));
    }
}
