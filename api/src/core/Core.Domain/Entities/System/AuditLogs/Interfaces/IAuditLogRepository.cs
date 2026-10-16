using Core.Library.Contracts.GenericRepositories;
using Core.Library.ResultPattern;

namespace Core.Domain.Entities.System.AuditLogs.Interfaces;

/// <summary>
/// Repository contract for <see cref="AuditLog"/> entities.
/// Extends <see cref="IRepository{AuditLog}"/> with audit-log-specific query operations.
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    /// <summary>
    /// Retrieves a paginated list of audit logs, optionally scoped to a specific entity.
    /// </summary>
    /// <param name="entityId">Optional entity ID. When provided, only logs for that entity are returned; when null, all logs are returned.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="sortBy">The field to sort by. Accepted values: <c>createdOnUtc</c> (default), <c>entityId</c>; any other value sorts by <c>createdOnUtc</c>.</param>
    /// <param name="isAscending">Sort direction: true for ascending, false for descending.</param>
    /// <param name="keyword">Optional keyword for partial match on the Description field.</param>
    /// <param name="fromDate">Optional lower bound on the Created timestamp (UTC).</param>
    /// <param name="toDate">Optional upper bound on the Created timestamp (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the paginated array of audit logs, scoped to <paramref name="entityId"/> when supplied (all logs otherwise).</returns>
    Task<Result<AuditLog[]>> GetPaginatedAsync(
        Guid? entityId,
        int page,
        int pageSize,
        string sortBy,
        bool isAscending,
        string? keyword,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of audit logs, optionally scoped to a specific entity, matching the given filters.
    /// </summary>
    /// <param name="entityId">Optional entity ID. When provided, only logs for that entity are counted; when null, all logs are counted.</param>
    /// <param name="keyword">Optional keyword for partial match on the Description field.</param>
    /// <param name="fromDate">Optional lower bound on the Created timestamp (UTC).</param>
    /// <param name="toDate">Optional upper bound on the Created timestamp (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the total count of matching audit logs.</returns>
    Task<Result<int>> CountFilteredAsync(
        Guid? entityId,
        string? keyword,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);
}
