using Core.Library.ResultPattern;

namespace App.UseCases.Samples.Notes;

internal sealed partial class NoteUseCase
{
    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteTransactionAsync(
            async () => await _noteService.DeleteAsync(id, cancellationToken),
            cancellationToken);
    }
}
