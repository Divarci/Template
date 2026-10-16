using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    private const int TitleMaxLength = 200;
    private const int ContentMaxLength = 5000;

    private static NoteCreateModel CreateValidNoteCreateModel(
        string title = "Test Note",
        string? content = "Test content",
        bool isPinned = false)
    {
        return new NoteCreateModel
        {
            Title = title,
            Content = content,
            IsPinned = isPinned
        };
    }

    private static Note CreateValidNote(
        string title = "Test Note",
        string? content = "Test content",
        bool isPinned = false)
    {
        Result<Note> result = Note.Create(CreateValidNoteCreateModel(title, content, isPinned));

        return result.Data!;
    }

    private static Note CreateDeletedNote()
    {
        Note note = CreateValidNote();

        note.SoftDelete();

        return note;
    }

    private static string CreateStringOfLength(int length)
    {
        return new string('A', length);
    }

    private static void AssertSuccessResult(Result result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }

    private static void AssertSuccessResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
        result.Data.Should().NotBeNull();
    }

    private static void AssertFailureResult(Result result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }

    private static void AssertFailureResult<T>(Result<T> result, HttpStatusCode expectedStatusCode)
    {
        result.IsFailureAndNoData.Should().BeTrue();
        result.StatusCode.Should().Be(expectedStatusCode);
    }
}
