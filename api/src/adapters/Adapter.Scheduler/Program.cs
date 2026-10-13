using Adapter.Scheduler;
using App.Processor;
using Core.Services;
using Infra.Caching;
using Infra.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .RegisterSchedulerLayer(builder.Configuration)
    .RegisterProcessorLayer()
    .RegisterServiceLayer()
    .RegisterCachingLayer(builder.Configuration)
    .RegisterPersistenceLayer(builder.Configuration);

var host = builder.Build();
host.Run();
