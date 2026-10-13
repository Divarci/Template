using Core.Library.ResultPattern;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infra.Persistence.Repositories.Generics;

internal partial class Repository<TEntity>
{
    public async Task<Result<int>> CountAsync(
        CancellationToken cancellationToken = default)
    {
        int count = await _context
            .Set<TEntity>()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<int>.Success(
            message: "Entity count retrieved successfully.",
            data: count,
            statusCode: HttpStatusCode.OK);
    }
}
