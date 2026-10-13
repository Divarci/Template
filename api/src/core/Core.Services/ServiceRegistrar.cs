using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Domain.Entities.System.OutboxMessages.Interfaces;
using Core.Services.Samples.Notes;
using Core.Services.System.AuditLogs;
using Core.Services.System.OutboxMessages;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Services;

public static class ServiceRegistrar
{
    public static IServiceCollection RegisterServiceLayer(this IServiceCollection services)
    {
        services.RegisterServices();

        return services;
    }

    private static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IOutboxMessageService, OutboxMessageService>();
    }
}
