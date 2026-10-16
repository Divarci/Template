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
    public async Task GetByIdAsync_WithExistingId_ReturnsSuccessResult()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();
        GivenOutboxMessageFound(message);

        // Act
        Result<OutboxMessage> result = await _sut.GetByIdAsync(message.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsOutboxMessage()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();
        GivenOutboxMessageFound(message);

        // Act
        Result<OutboxMessage> result = await _sut.GetByIdAsync(message.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        result.Data.Should().BeSameAs(message);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_CallsRepositoryWithId()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();
        GivenOutboxMessageFound(message);

        // Act
        await _sut.GetByIdAsync(message.Id, TestContext.Current.CancellationToken);

        // Assert
        _outboxMessageRepositoryMock.Verify(r => r.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOutboxMessageNotFound_ReturnsNotFoundError()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        GivenOutboxMessageNotFound(id);

        // Act
        Result<OutboxMessage> result = await _sut.GetByIdAsync(id, TestContext.Current.CancellationToken);

        // Assert
        AssertFailureResult(result, HttpStatusCode.NotFound);
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_DoesNotStageChanges()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();
        GivenOutboxMessageFound(message);

        // Act
        Result<OutboxMessage> result = await _sut.GetByIdAsync(message.Id, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResult(result, HttpStatusCode.OK);
        VerifyNoWriteSideEffects();
    }
}
