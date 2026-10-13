using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using Infra.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infra.Persistence.Repositories.Samples.Notes;

internal sealed partial class NoteRepository
{
    public async Task<Result<int>> CountFilteredAsync(
        string? keyword,
        bool? isPinned,
        CancellationToken cancellationToken = default)
    {
        int count = await _context.Set<Note>()
            .AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                EF.Functions.ILike(x.Title, $"%{keyword}%"))
            .WhereIf(isPinned.HasValue, x => x.IsPinned == isPinned!.Value)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<int>.Success(
            message: "Note count retrieved successfully.",
            data: count,
            statusCode: HttpStatusCode.OK);
    }
}
