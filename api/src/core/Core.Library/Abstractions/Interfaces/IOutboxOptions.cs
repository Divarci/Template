namespace Core.Library.Abstractions.Interfaces;

/// <summary>
/// Defines the configuration contract for the outbox processor, exposing
/// the polling interval and the maximum batch size used when draining
/// pending outbox messages.
/// </summary>
public interface IOutboxOptions
{
    /// <summary>
    /// Gets the interval, in seconds, between successive polls of the outbox.
    /// </summary>
    int IntervalInSeconds { get; }

    /// <summary>
    /// Gets the maximum number of outbox messages processed in a single batch.
    /// </summary>
    int BatchSize { get; }
}
