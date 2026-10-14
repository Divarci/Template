using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;

namespace Core.Domain.Tests;

internal static class NoteTestData
{
    public static NoteCreateModel CreateModel(
        string title = "Shopping list",
        string? content = "Milk, eggs",
        bool isPinned = false) =>
        new()
        {
            Title = title,
            Content = content,
            IsPinned = isPinned,
        };

    public static Note CreateNote(
        string title = "Shopping list",
        string? content = "Milk, eggs",
        bool isPinned = false) =>
        Note.Create(CreateModel(
            title,
            content,
            isPinned)).Data!;
}
