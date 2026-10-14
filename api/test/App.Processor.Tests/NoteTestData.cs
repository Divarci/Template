using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;

namespace App.Processor.Tests;

internal static class NoteTestData
{
    public static Note CreateNote() =>
        Note.Create(new NoteCreateModel
        {
            Title = "Shopping list",
            Content = "Milk, eggs",
            IsPinned = false,
        }).Data!;
}
