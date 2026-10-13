using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Infra.Persistence.Context;
using Infra.Persistence.Repositories.Generics;

namespace Infra.Persistence.Repositories.Samples.Notes;

internal sealed partial class NoteRepository(
    AppDbContext context) : Repository<Note>(context), INoteRepository
{
}
