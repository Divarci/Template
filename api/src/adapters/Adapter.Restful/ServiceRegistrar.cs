using Adapter.Restful.AspNetCore.Authentication;
using Adapter.Restful.AspNetCore.Diagnostics;
using Adapter.Restful.AspNetCore.Filters;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adapter.Restful;

public static class ServiceRegistrar
{
    public static IServiceCollection RegisterRestfulLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers(options => options.Filters.Add<ValidationFilter>());

        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

        services.AddOpenApi();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddApiVersioningInternal();

        services.AddAuthenticationInternal();

        services.AddHealthChecks();

        return services;
    }

    private static IServiceCollection AddApiVersioningInternal(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc();

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.ConfigureOptions<JwtBearerConfigurationOptions>();

        services.AddAuthorization(options =>
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        services.AddHttpContextAccessor();

        return services;
    }
}
