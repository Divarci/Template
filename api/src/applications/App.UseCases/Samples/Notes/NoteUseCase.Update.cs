using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;

namespace App.UseCases.Samples.Notes;

internal sealed partial class NoteUseCase
{
    public async Task<Result> UpdateAsync(
        Guid id,
        NoteUpdateModel updateModel,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteTransactionAsync(
            async () => await _noteService.UpdateAsync(id, updateModel, cancellationToken),
            cancellationToken);
    }
}
