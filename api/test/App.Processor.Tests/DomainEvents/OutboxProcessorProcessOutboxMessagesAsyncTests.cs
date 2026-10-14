using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace App.Processor.Tests.DomainEvents;

public sealed class OutboxProcessorProcessOutboxMessagesAsyncTests
{
    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenCalled_RunsInsideTransaction()
    {
        // Arrange
        OutboxProcessorTestSetup setup = new();
        setup.GivenPendingMessages();

        // Act
        await setup.Processor.ProcessOutboxMessagesAsync(
            10,
            OutboxMessageTestData.SerializerOptions,
            TestContext.Current.CancellationToken);

        // Assert
        setup.UnitOfWork.Verify(
            u => u.ExecuteTransactionAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenCalled_RequestsBatchOfRequestedSize()
    {
        // Arrange
        OutboxProcessorTestSetup setup = new();
        setup.GivenPendingMessages();

        // Act
        await setup.Processor.ProcessOutboxMessagesAsync(
            25,
            OutboxMessageTestData.SerializerOptions,
            TestContext.Current.CancellationToken);

        // Assert
        setup.OutboxMessageService.Verify(
            s => s.GetUnprocessedMessagesAsync(25, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenRetrievalFails_ReturnsRetrievalFailure()
    {
        // Arrange
        OutboxProcessorTestSetup setup = new();
        setup.OutboxMessageService
            .Setup(s => s.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<OutboxMessage>>.Failure(
                "Batch size must be greater than zero.",
                HttpStatusCode.InternalServerError));

        // Act
        Result result = await setup.Processor.ProcessOutboxMessagesAsync(
            0,
            OutboxMessageTestData.SerializerOptions,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Batch size must be greater than zero.");
        setup.ServiceScopeFactory.Verify(f => f.CreateScope(), Times.Never);
    }

    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenBatchIsProcessed_ReturnsOk()
    {
        // Arrange
        OutboxProcessorTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenNoteFound(note);
        setup.GivenPendingMessages(OutboxMessageTestData.CreateNoteCreatedMessage(note.Id));

        // Act
        Result result = await setup.Processor.ProcessOutboxMessagesAsync(
            10,
            OutboxMessageTestData.SerializerOptions,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Outbox messages processed successfully.");
    }

    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenMessageIsValid_DispatchesDeserialisedEventToHandler()
    {
        // Arrange
        OutboxProcessorTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenNoteFound(note);
        setup.GivenPendingMessages(OutboxMessageTestData.CreateNoteCreatedMessage(note.Id));

        // Act
        await setup.Processor.ProcessOutboxMessagesAsync(
            10,
            OutboxMessageTestData.SerializerOptions,
            TestContext.Current.CancellationToken);

        // Assert
        setup.NoteService.Verify(
            s => s.GetByIdAsync(
                note.Id,
                It.IsAny<Func<Note, Note>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenMessageIsValid_MarksMessageProcessedWithoutError()
    {
        // Arrange
        OutboxProcessorTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenNoteFound(note);
        OutboxMessage message = OutboxMessageTestData.CreateNoteCreatedMessage(note.Id);
        setup.GivenPendingMessages(message);

        // Act
        await setup.Processor.ProcessOutboxMessagesAsync(
            10,
            OutboxMessageTestData.SerializerOptions,
            TestContext.Current.CancellationToken);

        // Assert
        message.ProcessedOnUtc.Should().NotBeNull();
        message.Error.Should().BeNull();
    }

    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenTypeCannotBeResolved_RecordsErrorAndMarksProcessed()
    {
        // Arrange
        OutboxProcessorTestSetup setup = new();
        OutboxMessage message = OutboxMessageTestData.CreateMessage(
            type: "Unknown.Namespace.MissingDomainEvent, Unknown.Assembly",
            content: "{}");
        setup.GivenPendingMessages(message);

        // Act
        await setup.Processor.ProcessOutboxMessagesAsync(
            10,
            OutboxMessageTestData.SerializerOptions,
            TestContext.Current.CancellationToken);

        // Assert
        message.ProcessedOnUtc.Should().NotBeNull();
        message.Error.Should().Contain("Unable to resolve type: Unknown.Namespace.MissingDomainEvent, Unknown.Assembly");
    }

    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenHandlerThrows_RecordsErrorAndMarksProcessed()
    {
        // Arrange
        OutboxProcessorTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenNoteFound(note);
        OutboxMessage message = OutboxMessageTestData.CreateNoteCreatedMessage(note.Id);
        setup.GivenPendingMessages(message);
        setup.GivenHandlerThrows(new InvalidOperationException("Handler exploded"));

        // Act
        await setup.Processor.ProcessOutboxMessagesAsync(
            10,
            OutboxMessageTestData.SerializerOptions,
            TestContext.Current.CancellationToken);

        // Assert
        message.ProcessedOnUtc.Should().NotBeNull();
        message.Error.Should().Contain("Handler exploded");
    }

    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenOneMessageFails_ProcessesRemainingMessages()
    {
        // Arrange
        OutboxProcessorTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenNoteFound(note);
        OutboxMessage failingMessage = OutboxMessageTestData.CreateMessage(
            type: "Unknown.Namespace.MissingDomainEvent, Unknown.Assembly",
            content: "{}");
        OutboxMessage validMessage = OutboxMessageTestData.CreateNoteCreatedMessage(note.Id);
        setup.GivenPendingMessages(failingMessage, validMessage);

        // Act
        Result result = await setup.Processor.ProcessOutboxMessagesAsync(
            10,
            OutboxMessageTestData.SerializerOptions,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        validMessage.ProcessedOnUtc.Should().NotBeNull();
        validMessage.Error.Should().BeNull();
        setup.NoteService.Verify(
            s => s.GetByIdAsync(
                note.Id,
                It.IsAny<Func<Note, Note>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenHandlerThrows_LogsError()
    {
        // Arrange
        OutboxProcessorTestSetup setup = new();
        Note note = NoteTestData.CreateNote();
        setup.GivenNoteFound(note);
        setup.GivenPendingMessages(OutboxMessageTestData.CreateNoteCreatedMessage(note.Id));
        setup.GivenHandlerThrows(new InvalidOperationException("Handler exploded"));

        // Act
        await setup.Processor.ProcessOutboxMessagesAsync(
            10,
            OutboxMessageTestData.SerializerOptions,
            TestContext.Current.CancellationToken);

        // Assert
        setup.Logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
