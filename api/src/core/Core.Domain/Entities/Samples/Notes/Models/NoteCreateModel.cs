namespace Core.Domain.Entities.Samples.Notes.Models;

public sealed record NoteCreateModel
{
    public required string Title { get; init; }
    public required string? Content { get; init; }
    public required bool IsPinned { get; init; }
}
