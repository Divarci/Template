using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.DomainEvents;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Library.Contracts.DomainEvents.Handle;
using Core.Library.Contracts.UnitOfWorks;
using Core.Library.Exceptions;
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

    public override async Task HandleAsync(
        NoteCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Result handleResult = await _unitOfWork.ExecuteTransactionAsync(async () =>
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
                "NoteCreated event processed for note {NoteId}.",
                noteResult.Data.Id);

            return Result.Success(
                message: "Note created event handled successfully.",
                statusCode: HttpStatusCode.OK);
        }, cancellationToken);

        if (handleResult.IsFailure)
            throw new TemplateException(
                assemblyName: "App.Processor",
                className: nameof(NoteCreatedDomainEventHandler),
                methodName: nameof(HandleAsync),
                message: handleResult.Message);
    }
}
