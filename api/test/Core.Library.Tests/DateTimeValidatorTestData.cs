namespace Core.Library.Tests;

internal static class DateTimeValidatorTestData
{
    public static readonly DateTime JanuaryFirst = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime JanuaryThirtyFirst = new(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);
}
