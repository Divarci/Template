using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;

namespace Core.Domain.Entities.Samples.Notes.Interfaces;

/// <summary>
/// Service contract for <see cref="Note"/> entities. Performs the per-entity domain
/// orchestration for the note aggregate: it invokes the domain factory and behaviour
/// methods, stages changes on the note repository, writes audit-log entries via the
/// audit-log repository, and applies the cache-invalidation policy. It never commits —
/// the owning use case owns the transaction.
/// </summary>
public interface INoteService
{
    /// <summary>
    /// Creates a new note from the supplied model and records an audit-log entry.
    /// </summary>
    /// <param name="createModel">The data required to create the note.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the newly created <see cref="Note"/>.</returns>
    Task<Result<Note>> CreateAsync(
        NoteCreateModel createModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a partial update to an existing note and records an audit-log entry.
    /// Only the non-null fields of <paramref name="updateModel"/> are changed.
    /// </summary>
    /// <param name="id">The identifier of the note to update.</param>
    /// <param name="updateModel">The partial-update data; null fields are left unchanged.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating whether the update succeeded, carrying no data (204 No Content on success).</returns>
    Task<Result> UpdateAsync(
        Guid id,
        NoteUpdateModel updateModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes an existing note and records an audit-log entry.
    /// </summary>
    /// <param name="id">The identifier of the note to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating whether the soft-delete succeeded, carrying no data (204 No Content on success).</returns>
    Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a note by its unique identifier.
    /// </summary>
    /// <typeparam name="TDto">The DTO type produced by the mapper delegate.</typeparam>
    /// <param name="id">The note identifier.</param>
    /// <param name="mapper">A delegate that maps the <see cref="Note"/> entity to the target DTO.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the note if found.</returns>
    Task<Result<TDto>> GetByIdAsync<TDto>(
        Guid id,
        Func<Note, TDto> mapper,
        CancellationToken cancellationToken = default) where TDto : class;

    /// <summary>
    /// Retrieves a paginated list of notes matching the given filters.
    /// </summary>
    /// <typeparam name="TDto">The DTO type produced by the mapper delegate.</typeparam>
    /// <param name="page">The page number (1-based), already normalised.</param>
    /// <param name="pageSize">The number of items per page, already normalised.</param>
    /// <param name="sortBy">The field to sort by. Accepted values: <c>title</c> (default) and <c>isPinned</c>.</param>
    /// <param name="isAscending">Sort direction: true for ascending, false for descending.</param>
    /// <param name="keyword">Optional keyword for partial match on the Title field.</param>
    /// <param name="isPinned">Optional pin-state filter. When provided, only notes with the matching pin state are returned.</param>
    /// <param name="mapper">A delegate that maps an array of <see cref="Note"/> entities to an array of DTOs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated result containing the note DTOs.</returns>
    Task<PaginatedResult<TDto[]>> GetPaginatedAsync<TDto>(
        int page,
        int pageSize,
        string sortBy,
        bool isAscending,
        string? keyword,
        bool? isPinned,
        Func<Note[], TDto[]> mapper,
        CancellationToken cancellationToken = default) where TDto : class;
}
