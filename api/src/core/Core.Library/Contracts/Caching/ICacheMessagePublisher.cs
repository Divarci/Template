namespace Core.Library.Contracts.Caching;

/// <summary>
/// Publishes messages of type <typeparamref name="T"/> onto the distributed cache
/// pub/sub channel so that other processes can react to them.
/// </summary>
/// <typeparam name="T">The message payload type.</typeparam>
public interface ICacheMessagePublisher<T>
{
    /// <summary>
    /// Publishes a message to the cache pub/sub channel.
    /// </summary>
    /// <param name="message">The message payload to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task PublishAsync(T message, CancellationToken cancellationToken = default);
}
