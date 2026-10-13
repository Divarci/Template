using Core.Library.Contracts.Caching;
using Core.Library.Exceptions;
using Infra.Caching.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infra.Caching;

public static class ServiceRegistrar
{
    public static IServiceCollection RegisterCachingLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterRedisConnection(configuration);

        services.RegisterCacheService();

        return services;
    }

    private static void RegisterRedisConnection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new TemplateException(
                assemblyName: "Infra.Caching",
                className: nameof(ServiceRegistrar),
                methodName: nameof(RegisterCachingLayer),
                message: "Redis connection string 'ConnectionStrings:Redis' is not configured.");

        ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);

        options.AllowAdmin = true;
        options.AbortOnConnectFail = false;

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options));
    }

    private static void RegisterCacheService(this IServiceCollection services)
    {
        services.AddSingleton<CacheService>();

        services.AddSingleton<ICacheProvider>(sp => sp.GetRequiredService<CacheService>());

        services.AddSingleton<ICacheInvalidationService>(sp => sp.GetRequiredService<CacheService>());
    }
}
