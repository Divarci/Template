using App.UseCases.Samples.Notes;
using App.UseCases.System.AuditLogs;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace App.UseCases;

public static class ServiceRegistrar
{
    public static IServiceCollection RegisterUseCaseLayer(this IServiceCollection services)
    {
        services.RegisterUseCases();

        return services;
    }

    private static void RegisterUseCases(this IServiceCollection services)
    {
        services.AddScoped<INoteUseCase, NoteUseCase>();
        services.AddScoped<IAuditLogUseCase, AuditLogUseCase>();
    }
}
