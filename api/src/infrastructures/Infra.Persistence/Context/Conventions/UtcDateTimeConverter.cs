using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infra.Persistence.Context.Conventions;

// Npgsql maps DateTime to 'timestamp with time zone', which requires UTC values.
internal sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
