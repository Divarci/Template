using App.UseCases.Samples.Notes;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Library.Contracts.Caching;
using Core.Library.Contracts.UnitOfWorks;
using Core.Library.ResultPattern;
using Moq;

namespace App.UseCases.Tests;

internal sealed class NoteUseCaseTestSetup
{
    public NoteUseCaseTestSetup()
    {
        UseCase = new NoteUseCase(
            NoteService.Object,
            UnitOfWork.Object,
            CacheProvider.Object);
    }

    public Mock<INoteService> NoteService { get; } = new();

    public Mock<ITransactionalUnitOfWork> UnitOfWork { get; } = new();

    public Mock<ICacheProvider> CacheProvider { get; } = new();

    public NoteUseCase UseCase { get; }

    public void GivenTransactionRunsOperation()
    {
        UnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<Result>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<Task<Result>> operation, CancellationToken _) => operation());
    }

    public void GivenTransactionRunsOperation<T>()
    {
        UnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<Result<T>>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<Task<Result<T>>> operation, CancellationToken _) => operation());
    }

    public void GivenSingleCacheMiss()
    {
        CacheProvider
            .Setup(c => c.GetSingleAsync(
                It.IsAny<Func<Task<Result<string>>>>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>()))
            .Returns((Func<Task<Result<string>>> serviceMethod, bool _, string _, TimeSpan _) => serviceMethod());
    }

    public void GivenCollectionCacheMiss()
    {
        CacheProvider
            .Setup(c => c.GetPaginatedCollectionAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<Func<Task<PaginatedResult<string[]>>>>(),
                It.IsAny<TimeSpan>()))
            .Returns((string _, bool _, Func<Task<PaginatedResult<string[]>>> serviceCall, TimeSpan _) => serviceCall());
    }

    public void VerifyServiceCalledWith(
        int page,
        int pageSize,
        string sortBy,
        bool isAscending)
    {
        NoteService.Verify(
            s => s.GetPaginatedAsync(
                page,
                pageSize,
                sortBy,
                isAscending,
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<Func<Note[], string[]>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
