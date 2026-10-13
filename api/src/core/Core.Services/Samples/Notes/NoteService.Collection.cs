using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;

namespace Core.Services.Samples.Notes;

internal sealed partial class NoteService
{
    public async Task<PaginatedResult<TDto[]>> GetPaginatedAsync<TDto>(
        int page,
        int pageSize,
        string sortBy,
        bool isAscending,
        string? keyword,
        bool? isPinned,
        Func<Note[], TDto[]> mapper,
        CancellationToken cancellationToken = default) where TDto : class
    {
        Result<Note[]> entityResult = await _noteRepository
            .GetPaginatedAsync(
                page, pageSize, sortBy, isAscending, keyword, isPinned, cancellationToken);

        if (entityResult.IsFailureAndNoData)
            return PaginatedResult<TDto[]>.Failure(
                message: entityResult.Message,
                statusCode: entityResult.StatusCode);

        Result<int> totalCountResult = await _noteRepository
            .CountFilteredAsync(keyword, isPinned, cancellationToken);

        if (totalCountResult.IsFailure)
            return PaginatedResult<TDto[]>.Failure(
                message: totalCountResult.Message,
                statusCode: totalCountResult.StatusCode);

        return PaginatedResult<TDto[]>.Success(
            message: "List retrieved successfully",
            data: mapper(entityResult.Data),
            pageNumber: page,
            pageSize: pageSize,
            totalCount: totalCountResult.Data,
            pageCount: (int)Math.Ceiling((double)totalCountResult.Data / pageSize));
    }
}
