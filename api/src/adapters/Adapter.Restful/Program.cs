using Adapter.Restful;
using App.UseCases;
using Core.Services;
using Infra.Caching;
using Infra.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .RegisterRestfulLayer(builder.Configuration)
    .RegisterUseCaseLayer()
    .RegisterServiceLayer()
    .RegisterCachingLayer(builder.Configuration)
    .RegisterPersistenceLayer(builder.Configuration);

builder.Host.UseSerilog((hostingContext, configuration) =>
    configuration.ReadFrom.Configuration(hostingContext.Configuration));

var app = builder.Build();

app.MigratePersistence();

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
    app.MapOpenApi().AllowAnonymous();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
