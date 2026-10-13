using Core.Library.ResultPattern;

namespace Core.Domain.Entities.System.AuditLogs.Interfaces;

/// <summary>
/// Service contract for <see cref="AuditLog"/> entities.
/// Exposes read-only query operations on behalf of use cases; audit-log entries are
/// created by other entity services via the audit-log repository, never through this service.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Retrieves a paginated list of audit logs, optionally scoped to a specific entity.
    /// </summary>
    /// <typeparam name="TDto">The DTO type produced by the mapper delegate.</typeparam>
    /// <param name="entityId">Optional entity ID. When provided, only logs for that entity are returned; when null, all logs are returned.</param>
    /// <param name="page">The page number (1-based), already normalised.</param>
    /// <param name="pageSize">The number of items per page, already normalised.</param>
    /// <param name="sortBy">The field to sort by. Accepted values: <c>createdDate</c>, <c>description</c>.</param>
    /// <param name="isAscending">Sort direction: true for ascending, false for descending.</param>
    /// <param name="keyword">Optional keyword for partial match on the Description field.</param>
    /// <param name="fromDate">Optional lower bound on the Created timestamp (UTC).</param>
    /// <param name="toDate">Optional upper bound on the Created timestamp (UTC).</param>
    /// <param name="mapper">A delegate that maps an array of <see cref="AuditLog"/> entities to an array of DTOs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated result containing the audit log DTOs for the entity.</returns>
    Task<PaginatedResult<TDto[]>> GetPaginatedAsync<TDto>(
        Guid? entityId,
        int page,
        int pageSize,
        string sortBy,
        bool isAscending,
        string? keyword,
        DateTime? fromDate,
        DateTime? toDate,
        Func<AuditLog[], TDto[]> mapper,
        CancellationToken cancellationToken = default) where TDto : class;

    /// <summary>
    /// Retrieves an audit log by its unique identifier.
    /// </summary>
    /// <typeparam name="TDto">The DTO type produced by the mapper delegate.</typeparam>
    /// <param name="id">The audit log ID.</param>
    /// <param name="mapper">A delegate that maps the <see cref="AuditLog"/> entity to the target DTO.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the audit log if found.</returns>
    Task<Result<TDto>> GetByIdAsync<TDto>(
        Guid id,
        Func<AuditLog, TDto> mapper,
        CancellationToken cancellationToken = default) where TDto : class;
}
