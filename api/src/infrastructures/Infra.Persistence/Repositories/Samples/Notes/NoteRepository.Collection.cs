using Core.Domain.Entities.Samples.Notes;
using Core.Library.ResultPattern;
using Infra.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infra.Persistence.Repositories.Samples.Notes;

internal sealed partial class NoteRepository
{
    public async Task<Result<Note[]>> GetPaginatedAsync(
        int page,
        int pageSize,
        string sortBy,
        bool isAscending,
        string? keyword,
        bool? isPinned,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Note> query = _context.Set<Note>()
            .AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                EF.Functions.ILike(x.Title, $"%{keyword}%"))
            .WhereIf(isPinned.HasValue, x => x.IsPinned == isPinned!.Value);

        bool sortByPinned = string.Equals(sortBy, "isPinned", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<Note> orderedQuery = sortByPinned
            ? (isAscending
                ? query.OrderBy(x => x.IsPinned)
                : query.OrderByDescending(x => x.IsPinned))
            : (isAscending
                ? query.OrderBy(x => x.Title)
                : query.OrderByDescending(x => x.Title));

        Note[] notes = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return Result<Note[]>.Success(
            message: "Notes retrieved successfully.",
            data: notes,
            statusCode: HttpStatusCode.OK);
    }
}
