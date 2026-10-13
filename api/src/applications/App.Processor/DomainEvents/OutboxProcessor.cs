using Core.Domain.Entities.System.OutboxMessages;
using Core.Domain.Entities.System.OutboxMessages.Interfaces;
using Core.Library.Contracts.UnitOfWorks;
using Core.Library.ResultPattern;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace App.Processor.DomainEvents;

internal sealed partial class OutboxProcessor(
    ITransactionalUnitOfWork unitOfWork,
    IServiceScopeFactory serviceScopeFactory,
    IOutboxMessageService outboxMessageService,
    ILogger<OutboxProcessor> logger) : IOutboxProcessor
{
    private readonly ITransactionalUnitOfWork _unitOfWork = unitOfWork;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IOutboxMessageService _outboxMessageService = outboxMessageService;
    private readonly ILogger<OutboxProcessor> _logger = logger;

    public async Task<Result> ProcessOutboxMessagesAsync(
        int batchSize,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            string traceId = Guid.NewGuid().ToString();

            _logger.LogDebug("Outbox processing started. BatchSize: {BatchSize}.", batchSize);

            Result<IReadOnlyList<OutboxMessage>> outboxMessagesResult = await _outboxMessageService
                .GetUnprocessedMessagesAsync(batchSize, cancellationToken);

            if (outboxMessagesResult.IsFailureAndNoData)
            {
                _logger.LogWarning(
                    "Failed to retrieve unprocessed outbox messages. {Message} | TraceId: {TraceId}",
                    outboxMessagesResult.Message,
                    traceId);

                return Result.Failure(
                    message: outboxMessagesResult.Message,
                    statusCode: outboxMessagesResult.StatusCode);
            }

            foreach (OutboxMessage outboxMessage in outboxMessagesResult.Data)
            {
                _logger.LogDebug("Processing outbox message {MessageId}.", outboxMessage.Id);

                Exception? exception = null;

                try
                {
                    await InitialiseHandlersAsync(
                        outboxMessage,
                        jsonSerializerOptions,
                        _serviceScopeFactory,
                        cancellationToken);
                }
                catch (Exception caughtException)
                {
                    _logger.LogError(
                        caughtException,
                        "Domain event handler failed for outbox message {MessageId}. | TraceId: {TraceId}",
                        outboxMessage.Id,
                        traceId);

                    exception = caughtException;
                }

                await UpdateAsync(_outboxMessageService, outboxMessage, exception);
            }

            return Result.Success(
                message: "Outbox messages processed successfully.",
                statusCode: HttpStatusCode.OK);
        }, cancellationToken);
    }
}
