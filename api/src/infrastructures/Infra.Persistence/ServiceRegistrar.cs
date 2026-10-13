using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Domain.Entities.System.OutboxMessages.Interfaces;
using Core.Library.Contracts.GenericRepositories;
using Core.Library.Contracts.UnitOfWorks;
using Infra.Persistence.Context;
using Infra.Persistence.Repositories.Generics;
using Infra.Persistence.Repositories.Samples.Notes;
using Infra.Persistence.Repositories.System.AuditLogs;
using Infra.Persistence.Repositories.System.OutboxMessages;
using Infra.Persistence.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infra.Persistence;

public static class ServiceRegistrar
{
    public static IServiceCollection RegisterPersistenceLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterRepositories();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        return services;
    }

    public static IHost MigratePersistence(this IHost host)
    {
        using IServiceScope scope = host.Services.CreateScope();

        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Database.Migrate();

        return host;
    }

    private static void RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITransactionalUnitOfWork, UnitOfWork>();

        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
    }
}
