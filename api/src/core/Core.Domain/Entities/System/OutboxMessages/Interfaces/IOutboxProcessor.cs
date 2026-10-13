using Core.Library.ResultPattern;
using System.Text.Json;

namespace Core.Domain.Entities.System.OutboxMessages.Interfaces;

/// <summary>
/// Processor contract for <see cref="OutboxMessage"/> dispatch.
/// Drains pending outbox messages in batches and forwards them to their domain event handlers.
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Processes unprocessed outbox messages in batches.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to process in a single batch.</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options used to deserialize outbox messages.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Result"/> indicating whether the operation succeeded or failed.</returns>
    Task<Result> ProcessOutboxMessagesAsync(
        int batchSize,
        JsonSerializerOptions jsonSerializerOptions,
        CancellationToken cancellationToken = default);
}
