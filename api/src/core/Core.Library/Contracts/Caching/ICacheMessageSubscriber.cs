namespace Core.Library.Contracts.Caching;

/// <summary>
/// Subscribes to messages of type <typeparamref name="T"/> published onto the
/// distributed cache pub/sub channel and dispatches them to a supplied handler.
/// </summary>
/// <typeparam name="T">The message payload type.</typeparam>
public interface ICacheMessageSubscriber<T>
{
    /// <summary>
    /// Begins listening for messages on the cache pub/sub channel, invoking the
    /// supplied handler for each message received.
    /// </summary>
    /// <param name="handler">The callback invoked for each received message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous subscribe operation.</returns>
    Task SubscribeAsync(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops listening for messages on the cache pub/sub channel.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous unsubscribe operation.</returns>
    Task UnsubscribeAsync(CancellationToken cancellationToken = default);
}
