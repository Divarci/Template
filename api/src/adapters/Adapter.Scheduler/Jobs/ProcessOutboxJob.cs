using Adapter.Scheduler.Configuration;
using Core.Domain.Entities.System.OutboxMessages.Interfaces;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using Microsoft.Extensions.Options;
using Quartz;

namespace Adapter.Scheduler.Jobs;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxJob(
    IOutboxProcessor outboxProcessor,
    IOptions<OutboxOptions> options) : IJob
{
    private readonly IOutboxProcessor _outboxProcessor = outboxProcessor;
    private readonly OutboxOptions _outboxOptions = options.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        Result processResult = await _outboxProcessor
            .ProcessOutboxMessagesAsync(_outboxOptions.BatchSize);

        if (processResult.IsFailure)
            throw new TemplateException(
                assemblyName: "Adapter.Scheduler",
                className: nameof(ProcessOutboxJob),
                methodName: nameof(Execute),
                message: $"An error occurred while processing outbox messages. Details: {processResult.Message}");
    }
}
