using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace App.UseCases.Tests.Samples.Notes;

public sealed class NoteUseCaseUpdateAsyncTests
{
    [Fact]
    public async Task UpdateAsync_WhenCalled_DelegatesToServiceInsideTransaction()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenTransactionRunsOperation();
        Guid id = Guid.NewGuid();
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: "New title");

        // Act
        await setup.UseCase.UpdateAsync(
            id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        setup.UnitOfWork.Verify(
            u => u.ExecuteTransactionAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        setup.NoteService.Verify(
            s => s.UpdateAsync(
                id,
                updateModel,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTransactionDoesNotRunOperation_DoesNotCallService()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.UnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Transaction failed", HttpStatusCode.InternalServerError));
        Guid id = Guid.NewGuid();
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: "New title");

        // Act
        await setup.UseCase.UpdateAsync(
            id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        setup.NoteService.Verify(
            s => s.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<NoteUpdateModel>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenServiceCompletes_ReturnsServiceResult()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenTransactionRunsOperation();
        Guid id = Guid.NewGuid();
        NoteUpdateModel updateModel = NoteTestData.UpdateModel(title: "New title");
        Result serviceResult = Result.Success("Note updated successfully", HttpStatusCode.NoContent);
        setup.NoteService
            .Setup(s => s.UpdateAsync(
                id,
                updateModel,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        Result result = await setup.UseCase.UpdateAsync(
            id,
            updateModel,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(serviceResult);
    }
}
