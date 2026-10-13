using App.Processor.DomainEvents;
using Core.Domain.Entities.System.OutboxMessages.Interfaces;
using Core.Library.Contracts.DomainEvents.Handle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace App.Processor;

public static class ServiceRegistrar
{
    public static Assembly Assembly => typeof(ServiceRegistrar).Assembly;

    public static IServiceCollection RegisterProcessorLayer(this IServiceCollection services)
    {
        services.AddDomainEventHandlers();

        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        return services;
    }

    private static IServiceCollection AddDomainEventHandlers(this IServiceCollection services)
    {
        Type[] domainEventHandlers = [.. Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))];

        foreach (Type domainEventHandler in domainEventHandlers)
            services.TryAddScoped(domainEventHandler);

        return services;
    }
}
