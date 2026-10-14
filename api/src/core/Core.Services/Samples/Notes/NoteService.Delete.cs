using Core.Domain.Entities.Samples.Notes;
using Core.Library.Helpers;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Services.Samples.Notes;

internal sealed partial class NoteService
{
    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<Note> entityResult = await _noteRepository.GetByIdAsync(id, cancellationToken);

        if (entityResult.IsFailureAndNoData)
            return Result.Failure(
                message: entityResult.Message,
                statusCode: entityResult.StatusCode);

        Note note = entityResult.Data;

        Result softDeleteResult = note.SoftDelete();

        if (softDeleteResult.IsFailure)
            return softDeleteResult;

        _noteRepository.Update(note);

        await WriteAuditLogAsync(
            note.Id,
            softDeleteResult.Message,
            cancellationToken);

        await _cacheInvalidationService.InvalidateEntityAsync(
            CacheKeyHelper.Single(nameof(Note), ("id", note.Id)));

        await _cacheInvalidationService.InvalidateCollectionsAsync();

        return Result.Success(
            message: "Note deleted successfully",
            statusCode: HttpStatusCode.NoContent);
    }
}
