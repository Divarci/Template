using Core.Library.Contracts.GenericRepositories;
using Core.Library.ResultPattern;

namespace Core.Domain.Entities.System.OutboxMessages.Interfaces;

/// <summary>
/// Repository contract for <see cref="OutboxMessage"/> entities.
/// Extends <see cref="IRepository{OutboxMessage}"/> with batch retrieval of pending messages awaiting dispatch.
/// </summary>
public interface IOutboxMessageRepository : IRepository<OutboxMessage>
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
}
