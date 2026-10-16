using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;

namespace Core.Domain.Entities.Samples.Notes.Interfaces;

/// <summary>
/// Use case contract for <see cref="Note"/> entities. Orchestrates the note operations
/// exposed to the API surface: it wraps write operations in the unit-of-work transaction,
/// serves reads from the cache, and normalises paging and sorting inputs.
/// </summary>
public interface INoteUseCase
{
    /// <summary>
    /// Creates a new note within a transaction and records an audit-log entry.
    /// </summary>
    /// <param name="createModel">The data required to create the note.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result containing the newly created <see cref="Note"/>.</returns>
    Task<Result<Note>> CreateAsync(
        NoteCreateModel createModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a partial update to an existing note within a transaction and records one audit-log entry per changed field.
    /// Only the non-null fields of <paramref name="updateModel"/> are changed; when every field is null,
    /// nothing is staged, audited, or invalidated.
    /// </summary>
    /// <param name="id">The identifier of the note to update.</param>
    /// <param name="updateModel">The partial-update data; null fields are left unchanged.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating whether the update succeeded, carrying no data (204 No Content on success).</returns>
    Task<Result> UpdateAsync(
        Guid id,
        NoteUpdateModel updateModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes an existing note within a transaction and records an audit-log entry.
    /// </summary>
    /// <param name="id">The identifier of the note to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating whether the soft-delete succeeded, carrying no data (204 No Content on success).</returns>
    Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a note by its identifier, served from cache when available.
    /// </summary>
    /// <typeparam name="TDto">The DTO type produced by the mapper delegate.</typeparam>
    /// <param name="id">The note identifier.</param>
    /// <param name="mapper">A delegate that maps the <see cref="Note"/> entity to the target DTO.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result containing the note if found, or an error if not.</returns>
    Task<Result<TDto>> GetByIdAsync<TDto>(
        Guid id,
        Func<Note, TDto> mapper,
        CancellationToken cancellationToken = default) where TDto : class;

    /// <summary>
    /// Retrieves a paginated collection of notes, served from cache when available.
    /// </summary>
    /// <typeparam name="TDto">The DTO type produced by the mapper delegate.</typeparam>
    /// <param name="pageQuery">The raw page number from the query string (null defaults to 1).</param>
    /// <param name="pageSizeQuery">The raw page size from the query string (null defaults to 20).</param>
    /// <param name="sortByQuery">The raw sort field from the query string (null or blank defaults to <c>title</c>).</param>
    /// <param name="isAscendingQuery">The raw sort direction from the query string (null defaults to false — descending).</param>
    /// <param name="keyword">Optional keyword for partial match on the Title field.</param>
    /// <param name="isPinned">Optional pin-state filter. When provided, only notes with the matching pin state are returned.</param>
    /// <param name="mapper">A delegate that maps an array of <see cref="Note"/> entities to an array of DTOs.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paginated result containing the note DTOs.</returns>
    Task<PaginatedResult<TDto[]>> GetPaginatedAsync<TDto>(
        int? pageQuery,
        int? pageSizeQuery,
        string? sortByQuery,
        bool? isAscendingQuery,
        string? keyword,
        bool? isPinned,
        Func<Note[], TDto[]> mapper,
        CancellationToken cancellationToken = default) where TDto : class;
}
