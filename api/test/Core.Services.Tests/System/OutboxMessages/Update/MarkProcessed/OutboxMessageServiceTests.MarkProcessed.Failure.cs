using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.System.OutboxMessages;

public sealed partial class OutboxMessageServiceTests
{
    [Fact]
    public void MarkProcessed_WithNullMessage_ThrowsTemplateException()
    {
        // Arrange
        OutboxMessage message = null!;

        // Act
        Action act = () => _sut.MarkProcessed(message, null);

        // Assert
        act.Should().Throw<TemplateException>();
        VerifyNoWriteSideEffects();
    }

    [Fact]
    public void MarkProcessed_WhenRepositoryUpdateFails_ReturnsRepositoryFailure()
    {
        // Arrange
        OutboxMessage message = CreateValidOutboxMessage();
        const string failureMessage = "Failed to stage outbox message.";
        GivenOutboxMessageUpdateFails(failureMessage, HttpStatusCode.InternalServerError);

        // Act
        Result result = _sut.MarkProcessed(message, null);

        // Assert
        AssertFailureResult(result, HttpStatusCode.InternalServerError);
        result.Message.Should().Be(failureMessage);
        _outboxMessageRepositoryMock.Verify(r => r.Update(message), Times.Once);
    }
}
