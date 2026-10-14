using Core.Library.Contracts.GenericRepositories;
using Core.Library.ResultPattern;

namespace Core.Domain.Entities.Samples.Notes.Interfaces;

/// <summary>
/// Repository contract for <see cref="Note"/> entities.
/// Extends <see cref="IRepository{Note}"/> with note-specific query operations.
/// </summary>
public interface INoteRepository : IRepository<Note>
{
    /// <summary>
    /// Retrieves a paginated list of notes matching the given filters.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="sortBy">The field to sort by. Accepted values: <c>title</c> (default) and <c>isPinned</c>.</param>
    /// <param name="isAscending">Sort direction: true for ascending, false for descending.</param>
    /// <param name="keyword">Optional keyword for partial match on the Title field.</param>
    /// <param name="isPinned">Optional pin-state filter. When provided, only notes with the matching pin state are returned.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the paginated array of notes.</returns>
    Task<Result<Note[]>> GetPaginatedAsync(
        int page,
        int pageSize,
        string sortBy,
        bool isAscending,
        string? keyword,
        bool? isPinned,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of notes matching the given filters.
    /// </summary>
    /// <param name="keyword">Optional keyword for partial match on the Title field.</param>
    /// <param name="isPinned">Optional pin-state filter. When provided, only notes with the matching pin state are counted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the total count of matching notes.</returns>
    Task<Result<int>> CountFilteredAsync(
        string? keyword,
        bool? isPinned,
        CancellationToken cancellationToken = default);
}
