using Adapter.Scheduler.Configuration;
using Adapter.Scheduler.Jobs;
using Quartz;

namespace Adapter.Scheduler;

public static class ServiceRegistrar
{
    public static IServiceCollection RegisterSchedulerLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionKey));
        services.AddBackgroundServices(configuration);

        return services;
    }

    private static IServiceCollection AddBackgroundServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        OutboxOptions outboxOptions = configuration
            .GetSection(OutboxOptions.SectionKey)
            .Get<OutboxOptions>() ?? new OutboxOptions();

        services.AddQuartz(configurator =>
        {
            JobKey outboxJobKey = new(nameof(ProcessOutboxJob));

            configurator.AddJob<ProcessOutboxJob>(options => options.WithIdentity(outboxJobKey));

            configurator.AddTrigger(options => options
                .ForJob(outboxJobKey)
                .WithIdentity($"{nameof(ProcessOutboxJob)}-trigger")
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInSeconds(outboxOptions.IntervalInSeconds)
                    .RepeatForever()));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
