using Core.Library.Abstractions;
using Core.Library.Contracts.GenericRepositories;
using Infra.Persistence.Context;

namespace Infra.Persistence.Repositories.Generics;

internal partial class Repository<TEntity>(
    AppDbContext context) : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly AppDbContext _context = context;
}
