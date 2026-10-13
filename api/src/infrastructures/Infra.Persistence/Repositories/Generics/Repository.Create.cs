using Core.Library.ResultPattern;
using System.Net;

namespace Infra.Persistence.Repositories.Generics;

internal partial class Repository<TEntity>
{
    public async Task<Result<TEntity>> CreateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result<TEntity>.Failure(
                message: "The entity to create cannot be null.",
                statusCode: HttpStatusCode.InternalServerError);

        await _context
            .Set<TEntity>()
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        return Result<TEntity>.Success(
            message: "Entity created successfully.",
            data: entity,
            statusCode: HttpStatusCode.Created);
    }

    public async Task<Result> CreateRangeAsync(
        IEnumerable<TEntity> entityCollection,
        CancellationToken cancellationToken = default)
    {
        if (entityCollection is null)
            return Result.Failure(
                message: "The entity collection to create cannot be null.",
                statusCode: HttpStatusCode.InternalServerError);

        await _context
            .Set<TEntity>()
            .AddRangeAsync(entityCollection, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(
            message: "Entities created successfully.",
            statusCode: HttpStatusCode.Created);
    }
}
