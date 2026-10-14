using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;

namespace Adapter.Restful.Tests;

internal static class NoteTestData
{
    public static Note CreateNote(
        string title = "Shopping list",
        string? content = "Milk, eggs",
        bool isPinned = false) =>
        Note.Create(new NoteCreateModel
        {
            Title = title,
            Content = content,
            IsPinned = isPinned,
        }).Data!;
}
