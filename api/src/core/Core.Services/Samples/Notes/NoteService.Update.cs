using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.Helpers;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Services.Samples.Notes;

internal sealed partial class NoteService
{
    public async Task<Result> UpdateAsync(
        Guid id,
        NoteUpdateModel updateModel,
        CancellationToken cancellationToken = default)
    {
        Result<Note> entityResult = await _noteRepository.GetByIdAsync(id, cancellationToken);

        if (entityResult.IsFailureAndNoData)
            return entityResult;

        Note note = entityResult.Data;

        if (updateModel.Title is not null)
        {
            Result titleResult = note.UpdateTitle(updateModel.Title);

            if (titleResult.IsFailure)
                return titleResult;
        }

        if (updateModel.Content is not null)
        {
            Result contentResult = note.UpdateContent(updateModel.Content);

            if (contentResult.IsFailure)
                return contentResult;
        }

        if (updateModel.IsPinned is not null)
        {
            Result pinnedResult = note.UpdatePinned(updateModel.IsPinned.Value);

            if (pinnedResult.IsFailure)
                return pinnedResult;
        }

        _noteRepository.Update(note);

        Result updateResult = Result.Success(
            message: "Note updated successfully",
            statusCode: HttpStatusCode.NoContent);

        await WriteAuditLogAsync(
            note.Id,
            updateResult.Message,
            cancellationToken);

        await _cacheInvalidationService.InvalidateEntityAsync(
            CacheKeyHelper.Single(nameof(Note), ("id", note.Id)));

        await _cacheInvalidationService.InvalidateCollectionsAsync();

        return updateResult;
    }
}
