using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.DomainEvents;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Library.Contracts.DomainEvents.Handle;
using Core.Library.Contracts.UnitOfWorks;
using Core.Library.ResultPattern;
using Microsoft.Extensions.Logging;
using System.Net;

namespace App.Processor.DomainEvents.Samples.Notes;

internal sealed class NoteCreatedDomainEventHandler(
    ITransactionalUnitOfWork unitOfWork,
    INoteService noteService,
    ILogger<NoteCreatedDomainEventHandler> logger) : DomainEventHandler<NoteCreatedDomainEvent>
{
    private readonly ITransactionalUnitOfWork _unitOfWork = unitOfWork;
    private readonly INoteService _noteService = noteService;
    private readonly ILogger<NoteCreatedDomainEventHandler> _logger = logger;

    public override async Task Handle(
        NoteCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            Result<Note> noteResult = await _noteService.GetByIdAsync(
                domainEvent.NoteId,
                note => note,
                cancellationToken);

            if (noteResult.IsFailureAndNoData)
            {
                _logger.LogWarning(
                    "NoteCreated handler could not load note {NoteId}. {Message}",
                    domainEvent.NoteId,
                    noteResult.Message);

                return Result.Failure(
                    message: noteResult.Message,
                    statusCode: noteResult.StatusCode);
            }

            _logger.LogInformation(
                "NoteCreated event processed for note {NoteId} with title '{Title}'.",
                noteResult.Data.Id,
                noteResult.Data.Title);

            return Result.Success(
                message: "Note created event handled successfully.",
                statusCode: HttpStatusCode.OK);
        }, cancellationToken);
    }
}
