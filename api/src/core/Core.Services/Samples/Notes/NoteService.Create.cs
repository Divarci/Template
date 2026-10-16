using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;

namespace Core.Services.Samples.Notes;

internal sealed partial class NoteService
{
    public async Task<Result<Note>> CreateAsync(
        NoteCreateModel createModel,
        CancellationToken cancellationToken = default)
    {
        Result<Note> createResult = Note.Create(createModel);

        if (createResult.IsFailureAndNoData)
            return createResult;

        Result<Note> persistResult = await _noteRepository.CreateAsync(createResult.Data, cancellationToken);

        if (persistResult.IsFailureAndNoData)
            return persistResult;

        await WriteAuditLogAsync(
            createResult.Data.Id,
            createResult.Message,
            cancellationToken);

        await _cacheInvalidationService.InvalidateCollectionsAsync();

        return createResult;
    }
}
