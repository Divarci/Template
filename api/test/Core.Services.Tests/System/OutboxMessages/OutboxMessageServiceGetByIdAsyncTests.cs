using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.System.OutboxMessages;

public sealed class OutboxMessageServiceGetByIdAsyncTests
{
    [Fact]
    public async Task GetByIdAsync_WhenMessageExists_ReturnsRepositoryResult()
    {
        // Arrange
        OutboxMessageServiceTestSetup setup = new();
        OutboxMessage message = OutboxMessageTestData.CreateMessage();
        Result<OutboxMessage> repositoryResult = Result<OutboxMessage>.Success(
            message: "Message retrieved",
            data: message,
            statusCode: HttpStatusCode.OK);
        setup.OutboxMessageRepository
            .Setup(r => r.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(repositoryResult);

        // Act
        Result<OutboxMessage> result = await setup.Service.GetByIdAsync(message.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(repositoryResult);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMessageIsNotFound_PropagatesNotFound()
    {
        // Arrange
        OutboxMessageServiceTestSetup setup = new();
        Guid missingId = Guid.NewGuid();
        setup.OutboxMessageRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OutboxMessage>.Failure(
                message: "Outbox message not found.",
                statusCode: HttpStatusCode.NotFound));

        // Act
        Result<OutboxMessage> result = await setup.Service.GetByIdAsync(missingId, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
