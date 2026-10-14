using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.DomainEvents;
using Core.Library.Contracts.DomainEvents.Handle;
using Core.Library.ResultPattern;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace App.Processor.Tests.DomainEvents.Samples.Notes;

public sealed class NoteCreatedDomainEventHandlerHandleAsyncTests
{
    [Fact]
    public async Task HandleAsync_WhenCalled_RunsInsideTransaction()
    {
        // Arrange
        NoteCreatedDomainEventHandlerTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenNoteFound(note);

        // Act
        await setup.Handler.HandleAsync(new NoteCreatedDomainEvent(note.Id), TestContext.Current.CancellationToken);

        // Assert
        setup.UnitOfWork.Verify(
            u => u.ExecuteTransactionAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoteExists_LoadsNoteThroughService()
    {
        // Arrange
        NoteCreatedDomainEventHandlerTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenNoteFound(note);

        // Act
        await setup.Handler.HandleAsync(new NoteCreatedDomainEvent(note.Id), TestContext.Current.CancellationToken);

        // Assert
        setup.NoteService.Verify(
            s => s.GetByIdAsync(
                note.Id,
                It.IsAny<Func<Note, Note>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoteExists_CompletesTransactionWithSuccess()
    {
        // Arrange
        NoteCreatedDomainEventHandlerTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenNoteFound(note);

        // Act
        await setup.Handler.HandleAsync(new NoteCreatedDomainEvent(note.Id), TestContext.Current.CancellationToken);

        // Assert
        setup.TransactionResult.Should().NotBeNull();
        setup.TransactionResult!.IsSuccess.Should().BeTrue();
        setup.TransactionResult.Message.Should().Be("Note created event handled successfully.");
    }

    [Fact]
    public async Task HandleAsync_WhenNoteIsMissing_CompletesTransactionWithServiceFailure()
    {
        // Arrange
        NoteCreatedDomainEventHandlerTestSetup setup = new();
        Guid missingId = Guid.NewGuid();
        setup.GivenNoteMissing(missingId, "Note not found.");

        // Act
        await setup.Handler.HandleAsync(new NoteCreatedDomainEvent(missingId), TestContext.Current.CancellationToken);

        // Assert
        setup.TransactionResult.Should().NotBeNull();
        setup.TransactionResult!.IsFailure.Should().BeTrue();
        setup.TransactionResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
        setup.TransactionResult.Message.Should().Be("Note not found.");
    }

    [Fact]
    public async Task HandleAsync_WhenNoteIsMissing_LogsWarning()
    {
        // Arrange
        NoteCreatedDomainEventHandlerTestSetup setup = new();
        Guid missingId = Guid.NewGuid();
        setup.GivenNoteMissing(missingId, "Note not found.");

        // Act
        await setup.Handler.HandleAsync(new NoteCreatedDomainEvent(missingId), TestContext.Current.CancellationToken);

        // Assert
        setup.Logger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenInvokedThroughNonGenericContract_HandlesEvent()
    {
        // Arrange
        NoteCreatedDomainEventHandlerTestSetup setup = new();
        IDomainEventHandler handler = setup.Handler;
        Note note = NoteTestData.CreateNote();
        setup.GivenNoteFound(note);

        // Act
        await handler.HandleAsync(new NoteCreatedDomainEvent(note.Id), TestContext.Current.CancellationToken);

        // Assert
        setup.NoteService.Verify(
            s => s.GetByIdAsync(
                note.Id,
                It.IsAny<Func<Note, Note>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
