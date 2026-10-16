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

        List<string> auditMessages = [];

        if (updateModel.Title is not null)
        {
            Result titleResult = note.UpdateTitle(updateModel.Title);

            if (titleResult.IsFailure)
                return titleResult;

            auditMessages.Add(titleResult.Message);
        }

        if (updateModel.Content is not null)
        {
            Result contentResult = note.UpdateContent(updateModel.Content);

            if (contentResult.IsFailure)
                return contentResult;

            auditMessages.Add(contentResult.Message);
        }

        if (updateModel.IsPinned is not null)
        {
            Result pinnedResult = note.UpdatePinned(updateModel.IsPinned.Value);

            if (pinnedResult.IsFailure)
                return pinnedResult;

            auditMessages.Add(pinnedResult.Message);
        }

        if (auditMessages.Count == 0)
            return Result.Success(
                message: "No changes were applied to the note",
                statusCode: HttpStatusCode.NoContent);

        Result updateResult = _noteRepository.Update(note);

        if (updateResult.IsFailure)
            return updateResult;

        foreach (string auditMessage in auditMessages)
            await WriteAuditLogAsync(
                note.Id,
                auditMessage,
                cancellationToken);

        await _cacheInvalidationService.InvalidateEntityAsync(
            CacheKeyHelper.Single(nameof(Note), ("id", note.Id)));

        await _cacheInvalidationService.InvalidateCollectionsAsync();

        return Result.Success(
            message: "Note updated successfully",
            statusCode: HttpStatusCode.NoContent);
    }
}
