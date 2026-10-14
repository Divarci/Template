using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;

namespace App.UseCases.Tests;

internal static class NoteTestData
{
    public static readonly Func<Note, string> TitleMapper = note => note.Title;

    public static readonly Func<Note[], string[]> TitlesMapper = notes => [.. notes.Select(n => n.Title)];

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

    public static NoteUpdateModel UpdateModel(
        string? title = null,
        string? content = null,
        bool? isPinned = null) =>
        new()
        {
            Title = title,
            Content = content,
            IsPinned = isPinned,
        };
}
