using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Domain.Entities.System.AuditLogs;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Library.Contracts.Caching;
using Core.Library.ResultPattern;
using Core.Services.Samples.Notes;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace Core.Services.Tests;

internal sealed class NoteServiceTestSetup
{
    public NoteServiceTestSetup()
    {
        NoteRepository
            .Setup(r => r.CreateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Note note, CancellationToken _) => Result<Note>.Success(
                message: "Created",
                data: note,
                statusCode: HttpStatusCode.Created));

        AuditLogRepository
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditLog auditLog, CancellationToken _) => Result<AuditLog>.Success(
                message: "Created",
                data: auditLog,
                statusCode: HttpStatusCode.Created));

        Service = new NoteService(
            NoteRepository.Object,
            AuditLogRepository.Object,
            CacheInvalidationService.Object,
            Logger.Object);
    }

    public Mock<INoteRepository> NoteRepository { get; } = new();

    public Mock<IAuditLogRepository> AuditLogRepository { get; } = new();

    public Mock<ICacheInvalidationService> CacheInvalidationService { get; } = new();

    public Mock<ILogger<NoteService>> Logger { get; } = new();

    public NoteService Service { get; }

    public void GivenExistingNote(Note note)
    {
        NoteRepository
            .Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note>.Success(
                message: "Found",
                data: note,
                statusCode: HttpStatusCode.OK));
    }

    public void GivenNoteMissing(Guid id, string message)
    {
        NoteRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note>.Failure(
                message: message,
                statusCode: HttpStatusCode.NotFound));
    }

    public void GivenAuditLogStagingFails()
    {
        AuditLogRepository
            .Setup(r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AuditLog>.Failure(
                message: "Audit store unavailable",
                statusCode: HttpStatusCode.InternalServerError));
    }
}
