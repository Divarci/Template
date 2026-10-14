using AwesomeAssertions;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace App.UseCases.Tests.Samples.Notes;

public sealed class NoteUseCaseDeleteAsyncTests
{
    [Fact]
    public async Task DeleteAsync_WhenCalled_DelegatesToServiceInsideTransaction()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenTransactionRunsOperation();
        Guid id = Guid.NewGuid();

        // Act
        await setup.UseCase.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        setup.UnitOfWork.Verify(
            u => u.ExecuteTransactionAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        setup.NoteService.Verify(
            s => s.DeleteAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTransactionDoesNotRunOperation_DoesNotCallService()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.UnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Transaction failed", HttpStatusCode.InternalServerError));
        Guid id = Guid.NewGuid();

        // Act
        await setup.UseCase.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        setup.NoteService.Verify(
            s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenServiceCompletes_ReturnsServiceResult()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenTransactionRunsOperation();
        Guid id = Guid.NewGuid();
        Result serviceResult = Result.Success("Note deleted successfully", HttpStatusCode.NoContent);
        setup.NoteService
            .Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        Result result = await setup.UseCase.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(serviceResult);
    }
}
