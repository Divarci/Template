using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Domain.Entities.System.OutboxMessages.Interfaces;
using Core.Library.ResultPattern;
using Core.Services.System.OutboxMessages;
using Moq;
using System.Net;

namespace Core.Services.Tests.System.OutboxMessages;

public sealed partial class OutboxMessageServiceTests
{
    private const string MessageType = "Core.Domain.Entities.Samples.Notes.DomainEvents.NoteCreatedDomainEvent, Core.Domain";
    private const string MessageContent = "{\"noteId\":\"3f2504e0-4f89-11d3-9a0c-0305e82c3301\"}";

    private readonly Mock<IOutboxMessageRepository> _outboxMessageRepositoryMock = new();
    private readonly OutboxMessageService _sut;

    public OutboxMessageServiceTests()
    {
        _sut = new OutboxMessageService(_outboxMessageRepositoryMock.Object);
    }

    private static OutboxMessage CreateValidOutboxMessage()
    {
        Result<OutboxMessage> result = OutboxMessage.Create(Guid.NewGuid(), MessageType, MessageContent, DateTime.UtcNow);

        return result.Data!;
    }

    private void GivenOutboxMessageFound(OutboxMessage message)
    {
        _outboxMessageRepositoryMock
            .Setup(r => r.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OutboxMessage>.Success(
                message: "Outbox message retrieved successfully",
                data: message,
                statusCode: HttpStatusCode.OK));
    }

    private void GivenOutboxMessageNotFound(Guid id)
    {
        _outboxMessageRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OutboxMessage>.Failure(
                message: "Outbox message not found.",
                statusCode: HttpStatusCode.NotFound));
    }

    private void GivenOutboxMessageUpdateSucceeds()
    {
        _outboxMessageRepositoryMock
            .Setup(r => r.Update(It.IsAny<OutboxMessage>()))
            .Returns(Result.Success(
                message: "Outbox message staged",
                statusCode: HttpStatusCode.OK));
    }

    private void GivenOutboxMessageUpdateFails(string message, HttpStatusCode statusCode)
    {
        _outboxMessageRepositoryMock
            .Setup(r => r.Update(It.IsAny<OutboxMessage>()))
            .Returns(Result.Failure(
                message: message,
                statusCode: statusCode));
    }

    private void GivenUnprocessedMessagesFound(IReadOnlyList<OutboxMessage> messages)
    {
        _outboxMessageRepositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<OutboxMessage>>.Success(
                message: "Unprocessed messages retrieved successfully",
                data: messages,
                statusCode: HttpStatusCode.OK));
    }

    private void GivenUnprocessedMessagesFail(string message, HttpStatusCode statusCode)
    {
        _outboxMessageRepositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<OutboxMessage>>.Failure(
                message: message,
                statusCode: statusCode));
    }

    private void VerifyNoWriteSideEffects()
    {
        _outboxMessageRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _outboxMessageRepositoryMock.Verify(r => r.Update(It.IsAny<OutboxMessage>()), Times.Never);
        _outboxMessageRepositoryMock.Verify(r => r.Delete(It.IsAny<OutboxMessage>()), Times.Never);
    }

    private static void AssertSuccessResult(Result result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }

    private static void AssertSuccessResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Data.Should().NotBeNull();
    }

    private static void AssertFailureResult(Result result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }

    private static void AssertFailureResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailureAndNoData.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }
}
