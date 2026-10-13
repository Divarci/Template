using Core.Library.ResultPattern;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infra.Persistence.Repositories.Generics;

internal partial class Repository<TEntity>
{
    public async Task<Result<TEntity>> GetByIdAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        TEntity? entity = await _context
            .Set<TEntity>()
            .SingleOrDefaultAsync(e => e.Id == entityId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            return Result<TEntity>.Failure(
                message: $"Entity with id {entityId} not found.",
                statusCode: HttpStatusCode.NotFound);

        return Result<TEntity>.Success(
            message: $"Entity with id {entityId} retrieved successfully.",
            data: entity,
            statusCode: HttpStatusCode.OK);
    }
}
