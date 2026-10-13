using Core.Library.ResultPattern;
using System.Net;

namespace Infra.Persistence.Repositories.Generics;

internal partial class Repository<TEntity>
{
    public Result Delete(TEntity entity)
    {
        if (entity is null)
            return Result.Failure(
                message: "The entity to delete cannot be null.",
                statusCode: HttpStatusCode.InternalServerError);

        _context
            .Set<TEntity>()
            .Remove(entity);

        return Result.Success(
            message: "Entity deleted successfully.",
            statusCode: HttpStatusCode.OK);
    }

    public Result DeleteRange(IEnumerable<TEntity> entityCollection)
    {
        if (entityCollection is null)
            return Result.Failure(
                message: "The entity collection to delete cannot be null.",
                statusCode: HttpStatusCode.InternalServerError);

        _context
            .Set<TEntity>()
            .RemoveRange(entityCollection);

        return Result.Success(
            message: "Entities deleted successfully.",
            statusCode: HttpStatusCode.OK);
    }
}
