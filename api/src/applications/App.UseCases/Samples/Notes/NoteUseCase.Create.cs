using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;

namespace App.UseCases.Samples.Notes;

internal sealed partial class NoteUseCase
{
    public async Task<Result<Note>> CreateAsync(
        NoteCreateModel createModel,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteTransactionAsync(
            async () => await _noteService.CreateAsync(createModel, cancellationToken),
            cancellationToken);
    }
}
