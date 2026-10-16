using Core.Library.ResultPattern;

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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Result"/> indicating whether the operation succeeded or failed.</returns>
    Task<Result> ProcessOutboxMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default);
}
