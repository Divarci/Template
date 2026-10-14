using AwesomeAssertions;
using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.System.OutboxMessages;

public sealed class OutboxMessageServiceGetUnprocessedMessagesAsyncTests
{
    [Fact]
    public async Task GetUnprocessedMessagesAsync_WhenBatchSizeIsPositive_ReturnsRepositoryResult()
    {
        // Arrange
        OutboxMessageServiceTestSetup setup = new();
        OutboxMessage message = OutboxMessageTestData.CreateMessage();
        Result<IReadOnlyList<OutboxMessage>> repositoryResult = Result<IReadOnlyList<OutboxMessage>>.Success(
            message: "Messages retrieved",
            data: [message],
            statusCode: HttpStatusCode.OK);
        setup.OutboxMessageRepository
            .Setup(r => r.GetUnprocessedMessagesAsync(25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(repositoryResult);

        // Act
        Result<IReadOnlyList<OutboxMessage>> result = await setup.Service.GetUnprocessedMessagesAsync(
            25,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(repositoryResult);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetUnprocessedMessagesAsync_WhenBatchSizeIsNotPositive_ReturnsFailureWithoutQuerying(int batchSize)
    {
        // Arrange
        OutboxMessageServiceTestSetup setup = new();

        // Act
        Result<IReadOnlyList<OutboxMessage>> result = await setup.Service.GetUnprocessedMessagesAsync(
            batchSize,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Batch size must be greater than zero.");
        setup.OutboxMessageRepository.Verify(
            r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
