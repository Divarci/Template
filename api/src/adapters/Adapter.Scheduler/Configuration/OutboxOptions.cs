using Core.Library.Abstractions.Interfaces;

namespace Adapter.Scheduler.Configuration;

internal sealed record OutboxOptions : IOutboxOptions
{
    public const string SectionKey = "Outbox";

    public int IntervalInSeconds { get; init; }

    public int BatchSize { get; init; }
}
