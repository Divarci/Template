using Core.Library.Abstractions;
using System.Linq.Expressions;

namespace Infra.Persistence.Extensions;

internal static class IQueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate) where T : BaseEntity
        => condition
            ? query.Where(predicate)
            : query;
}
