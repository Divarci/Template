using Core.Library.ResultPattern;
using System.Net;

namespace Infra.Persistence.Repositories.Generics;

internal partial class Repository<TEntity>
{
    public Result Update(TEntity entity)
    {
        if (entity is null)
            return Result.Failure(
                message: "The entity to update cannot be null.",
                statusCode: HttpStatusCode.InternalServerError);

        _context
            .Set<TEntity>()
            .Update(entity);

        return Result.Success(
            message: "Entity updated successfully.",
            statusCode: HttpStatusCode.OK);
    }

    public Result UpdateRange(IEnumerable<TEntity> entityCollection)
    {
        if (entityCollection is null)
            return Result.Failure(
                message: "The entity collection to update cannot be null.",
                statusCode: HttpStatusCode.InternalServerError);

        _context
            .Set<TEntity>()
            .UpdateRange(entityCollection);

        return Result.Success(
            message: "Entities updated successfully.",
            statusCode: HttpStatusCode.OK);
    }
}
