using App.UseCases.System.AuditLogs;
using Core.Domain.Entities.System.AuditLogs;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Library.Contracts.Caching;
using Core.Library.ResultPattern;
using Moq;

namespace App.UseCases.Tests;

internal sealed class AuditLogUseCaseTestSetup
{
    public AuditLogUseCaseTestSetup()
    {
        UseCase = new AuditLogUseCase(
            AuditLogService.Object,
            CacheProvider.Object);
    }

    public Mock<IAuditLogService> AuditLogService { get; } = new();

    public Mock<ICacheProvider> CacheProvider { get; } = new();

    public AuditLogUseCase UseCase { get; }

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
        AuditLogService.Verify(
            s => s.GetPaginatedAsync(
                It.IsAny<Guid?>(),
                page,
                pageSize,
                sortBy,
                isAscending,
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<Func<AuditLog[], string[]>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
