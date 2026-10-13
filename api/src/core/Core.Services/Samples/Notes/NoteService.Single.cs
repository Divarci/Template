using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;

namespace Core.Services.Samples.Notes;

internal sealed partial class NoteService
{
    public async Task<Result<TDto>> GetByIdAsync<TDto>(
        Guid id,
        Func<Note, TDto> mapper,
        CancellationToken cancellationToken = default) where TDto : class
    {
        Result<Note> entityResult = await _noteRepository
            .GetByIdAsync(id, cancellationToken);

        if (entityResult.IsFailureAndNoData)
            return Result<TDto>.Failure(
                message: entityResult.Message,
                statusCode: entityResult.StatusCode);

        return Result<TDto>.Success(
            message: entityResult.Message,
            data: mapper(entityResult.Data),
            statusCode: entityResult.StatusCode);
    }
}
