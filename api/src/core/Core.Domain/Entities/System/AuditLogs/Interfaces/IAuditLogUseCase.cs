using Core.Library.ResultPattern;

namespace Core.Domain.Entities.System.AuditLogs.Interfaces;

/// <summary>
/// Use case contract for <see cref="AuditLog"/> entities.
/// Orchestrates read-only query operations exposed to the API surface, normalising paging and sorting inputs.
/// </summary>
public interface IAuditLogUseCase
{
    /// <summary>
    /// Retrieves a paginated collection of audit logs, optionally scoped to a specific entity.
    /// </summary>
    /// <typeparam name="TDto">The DTO type produced by the mapper delegate.</typeparam>
    /// <param name="entityId">Optional entity ID. When provided, only logs for that entity are returned; when null, all logs are returned.</param>
    /// <param name="pageQuery">The raw page number from the query string (null defaults to 1).</param>
    /// <param name="pageSizeQuery">The raw page size from the query string (null defaults to 20).</param>
    /// <param name="sortByQuery">The raw sort field from the query string (null defaults to <c>createdDate</c>).</param>
    /// <param name="isAscendingQuery">The raw sort direction from the query string (null defaults to false — most recent first).</param>
    /// <param name="keyword">Optional keyword for partial match on the Description field.</param>
    /// <param name="fromDate">Optional lower bound on the Created timestamp (UTC).</param>
    /// <param name="toDate">Optional upper bound on the Created timestamp (UTC).</param>
    /// <param name="mapper">A delegate that maps an array of <see cref="AuditLog"/> entities to an array of DTOs.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated result containing the audit log DTOs for the entity.</returns>
    Task<PaginatedResult<TDto[]>> GetPaginatedAsync<TDto>(
        Guid? entityId,
        int? pageQuery,
        int? pageSizeQuery,
        string? sortByQuery,
        bool? isAscendingQuery,
        string? keyword,
        DateTime? fromDate,
        DateTime? toDate,
        Func<AuditLog[], TDto[]> mapper,
        CancellationToken cancellationToken = default) where TDto : class;

    /// <summary>
    /// Retrieves an audit log by its ID.
    /// </summary>
    /// <typeparam name="TDto">The DTO type produced by the mapper delegate.</typeparam>
    /// <param name="id">The audit log ID.</param>
    /// <param name="mapper">A delegate that maps the <see cref="AuditLog"/> entity to the target DTO.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result containing the audit log if found, or an error if not.</returns>
    Task<Result<TDto>> GetByIdAsync<TDto>(
        Guid id,
        Func<AuditLog, TDto> mapper,
        CancellationToken cancellationToken = default) where TDto : class;
}
