using Core.Domain.Entities.Samples.Notes;
using Core.Library.Helpers;
using Core.Library.ResultPattern;

namespace App.UseCases.Samples.Notes;

internal sealed partial class NoteUseCase
{
    public async Task<Result<TDto>> GetByIdAsync<TDto>(
        Guid id,
        Func<Note, TDto> mapper,
        CancellationToken cancellationToken = default) where TDto : class
    {
        string cacheKey = CacheKeyHelper.Single(nameof(Note), ("id", id));

        Result<TDto> noteResult = await _cacheProvider.GetSingleAsync(
            serviceMethod: async () => await _noteService.GetByIdAsync(
                id, mapper, cancellationToken),
            useCache: true,
            cacheKey: cacheKey,
            _timeout);

        return noteResult;
    }
}
