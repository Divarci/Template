using Core.Library.ResultPattern;

namespace Core.Domain.Entities.System.OutboxMessages.Interfaces;

/// <summary>
/// Service contract for <see cref="OutboxMessage"/> entities.
/// Retrieves pending and individual outbox records and marks them as processed on behalf of the outbox processor.
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

    /// <summary>
    /// Marks an already-loaded outbox message as processed, recording the handler error if any,
    /// and stages the change on the repository. The operation is synchronous: it performs no I/O,
    /// and the change is persisted when the caller's transaction commits.
    /// </summary>
    /// <param name="message">The outbox message to mark as processed.</param>
    /// <param name="error">The handler error details, or <see langword="null"/> when every handler succeeded.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating whether the message was marked and staged successfully;
    /// on success it carries the domain operation's message and a <c>204 No Content</c> status.
    /// </returns>
    Result MarkProcessed(
        OutboxMessage message,
        string? error);
}
