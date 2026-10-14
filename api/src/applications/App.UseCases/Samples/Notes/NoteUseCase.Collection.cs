using Core.Domain.Entities.Samples.Notes;
using Core.Library.Helpers;
using Core.Library.ResultPattern;

namespace App.UseCases.Samples.Notes;

internal sealed partial class NoteUseCase
{
    public async Task<PaginatedResult<TDto[]>> GetPaginatedAsync<TDto>(
        int? pageQuery,
        int? pageSizeQuery,
        string? sortByQuery,
        bool? isAscendingQuery,
        string? keyword,
        bool? isPinned,
        Func<Note[], TDto[]> mapper,
        CancellationToken cancellationToken = default) where TDto : class
    {
        int page = pageQuery < 1 || pageQuery is null ? 1 : pageQuery.Value;
        int size = pageSizeQuery < 1 || pageSizeQuery is null ? 20 : pageSizeQuery.Value;
        string sortBy = string.IsNullOrWhiteSpace(sortByQuery) ? "title" : sortByQuery;
        bool ascending = isAscendingQuery ?? false;

        string cacheKey = CacheKeyHelper.Collection(
            nameof(Note),
            nameof(GetPaginatedAsync),
            [page, size, sortBy, ascending,
             keyword ?? string.Empty,
             isPinned?.ToString() ?? string.Empty]);

        PaginatedResult<TDto[]> retrievalResult = await _cacheProvider.GetPaginatedCollectionAsync(
            cacheKey: cacheKey,
            useCache: true,
            serviceCall: async () => await _noteService.GetPaginatedAsync(
                page, size, sortBy, ascending, keyword, isPinned, mapper, cancellationToken),
            _timeout);

        return retrievalResult;
    }
}
