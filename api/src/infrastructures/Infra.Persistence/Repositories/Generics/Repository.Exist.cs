using Core.Library.ResultPattern;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infra.Persistence.Repositories.Generics;

internal partial class Repository<TEntity>
{
    public async Task<Result<bool>> ExistsAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        bool exists = await _context
            .Set<TEntity>()
            .AnyAsync(e => e.Id == entityId, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
            return Result<bool>.Failure(
                message: $"Entity with id {entityId} not found.",
                statusCode: HttpStatusCode.NotFound);

        return Result<bool>.Success(
            message: $"Entity with id {entityId} exists.",
            data: true,
            statusCode: HttpStatusCode.OK);
    }
}
