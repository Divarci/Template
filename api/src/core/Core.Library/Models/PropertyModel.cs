namespace Core.Library.Models;

public sealed record PropertyModel<T>
{
    public required T? Value { get; init; }
}
