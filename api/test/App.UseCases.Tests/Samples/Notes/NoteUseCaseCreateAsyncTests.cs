using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;
using Moq;
using System.Net;
using Xunit;

namespace App.UseCases.Tests.Samples.Notes;

public sealed class NoteUseCaseCreateAsyncTests
{
    [Fact]
    public async Task CreateAsync_WhenCalled_DelegatesToServiceInsideTransaction()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenTransactionRunsOperation<Note>();
        NoteCreateModel createModel = NoteTestData.CreateModel();

        // Act
        await setup.UseCase.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        setup.UnitOfWork.Verify(
            u => u.ExecuteTransactionAsync(It.IsAny<Func<Task<Result<Note>>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        setup.NoteService.Verify(
            s => s.CreateAsync(createModel, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenTransactionDoesNotRunOperation_DoesNotCallService()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.UnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task<Result<Note>>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Note>.Failure("Transaction failed", HttpStatusCode.InternalServerError));
        NoteCreateModel createModel = NoteTestData.CreateModel();

        // Act
        await setup.UseCase.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        setup.NoteService.Verify(
            s => s.CreateAsync(It.IsAny<NoteCreateModel>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenServiceSucceeds_ReturnsServiceResult()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        setup.GivenTransactionRunsOperation<Note>();
        NoteCreateModel createModel = NoteTestData.CreateModel();
        Result<Note> serviceResult = Note.Create(createModel);
        setup.NoteService
            .Setup(s => s.CreateAsync(createModel, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        // Act
        Result<Note> result = await setup.UseCase.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(serviceResult);
    }

    [Fact]
    public async Task CreateAsync_WhenTransactionFails_ReturnsTransactionResult()
    {
        // Arrange
        NoteUseCaseTestSetup setup = new();
        Result<Note> transactionResult = Result<Note>.Failure("Transaction failed", HttpStatusCode.InternalServerError);
        setup.UnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task<Result<Note>>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionResult);
        NoteCreateModel createModel = NoteTestData.CreateModel();

        // Act
        Result<Note> result = await setup.UseCase.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(transactionResult);
    }
}
