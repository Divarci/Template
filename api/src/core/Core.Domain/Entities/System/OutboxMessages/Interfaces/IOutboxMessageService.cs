using Core.Library.ResultPattern;

namespace Core.Domain.Entities.System.OutboxMessages.Interfaces;

/// <summary>
/// Service contract for <see cref="OutboxMessage"/> entities.
/// Coordinates retrieval of pending and individual outbox records on behalf of the outbox processor.
/// </summary>
public interface IOutboxMessageService
{
    /// <summary>
    /// Retrieves a batch of unprocessed outbox messages pending dispatch.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of unprocessed <see cref="OutboxMessage"/> instances.</returns>
    Task<Result<IReadOnlyList<OutboxMessage>>> GetUnprocessedMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an outbox message by its unique identifier.
    /// </summary>
    /// <param name="id">The outbox message ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the outbox message if found.</returns>
    Task<Result<OutboxMessage>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
