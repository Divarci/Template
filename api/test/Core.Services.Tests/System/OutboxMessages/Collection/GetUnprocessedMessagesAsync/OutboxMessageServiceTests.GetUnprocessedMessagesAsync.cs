using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.System.OutboxMessages;

public sealed partial class OutboxMessageServiceTests
{
    [Fact]
    public async Task GetUnprocessedMessagesAsync_WithValidParameters_ReturnsSuccessResult()
    {
        // Arrange
        int batchSize = 10;
        IReadOnlyList<OutboxMessage> messages = [CreateValidOutboxMessage(), CreateValidOutboxMessage()];
        GivenUnprocessedMessagesFound(messages);

        // Act
        Result<IReadOnlyList<OutboxMessage>> result = await _sut.GetUnprocessedMessagesAsync(batchSize, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Data.Should().BeSameAs(messages);
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_WithValidParameters_CallsRepositoryWithParameters()
    {
        // Arrange
        int batchSize = 25;
        IReadOnlyList<OutboxMessage> messages = [CreateValidOutboxMessage()];
        GivenUnprocessedMessagesFound(messages);

        // Act
        await _sut.GetUnprocessedMessagesAsync(batchSize, TestContext.Current.CancellationToken);

        // Assert
        _outboxMessageRepositoryMock.Verify(
            r => r.GetUnprocessedMessagesAsync(batchSize, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetUnprocessedMessagesAsync_WithNonPositiveBatchSize_ReturnsInternalServerError(int batchSize)
    {
        // Act
        Result<IReadOnlyList<OutboxMessage>> result = await _sut.GetUnprocessedMessagesAsync(batchSize, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Batch size must be greater than zero.");
        _outboxMessageRepositoryMock.Verify(
            r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_WithEmptyResult_ReturnsSuccessWithEmptyCollection()
    {
        // Arrange
        int batchSize = 10;
        IReadOnlyList<OutboxMessage> messages = [];
        GivenUnprocessedMessagesFound(messages);

        // Act
        Result<IReadOnlyList<OutboxMessage>> result = await _sut.GetUnprocessedMessagesAsync(batchSize, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_WhenRepositoryFails_ReturnsRepositoryFailure()
    {
        // Arrange
        int batchSize = 10;
        const string failureMessage = "Failed to retrieve unprocessed messages.";
        GivenUnprocessedMessagesFail(failureMessage, HttpStatusCode.InternalServerError);

        // Act
        Result<IReadOnlyList<OutboxMessage>> result = await _sut.GetUnprocessedMessagesAsync(batchSize, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be(failureMessage);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_WithValidParameters_DoesNotStageChanges()
    {
        // Arrange
        int batchSize = 10;
        IReadOnlyList<OutboxMessage> messages = [CreateValidOutboxMessage()];
        GivenUnprocessedMessagesFound(messages);

        // Act
        Result<IReadOnlyList<OutboxMessage>> result = await _sut.GetUnprocessedMessagesAsync(batchSize, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        VerifyNoWriteSideEffects();
    }
}
