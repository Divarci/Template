using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infra.Persistence.Context.Conventions;

// Npgsql maps DateTime to 'timestamp with time zone', which requires UTC values.
internal sealed class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter()
        : base(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {
    }
}
