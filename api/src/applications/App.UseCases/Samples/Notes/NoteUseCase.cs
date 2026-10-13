using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Library.Contracts.Caching;
using Core.Library.Contracts.UnitOfWorks;

namespace App.UseCases.Samples.Notes;

internal sealed partial class NoteUseCase(
    INoteService noteService,
    ITransactionalUnitOfWork unitOfWork,
    ICacheProvider cacheProvider) : INoteUseCase
{
    private readonly INoteService _noteService = noteService;
    private readonly ITransactionalUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheProvider _cacheProvider = cacheProvider;

    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(30);
}
