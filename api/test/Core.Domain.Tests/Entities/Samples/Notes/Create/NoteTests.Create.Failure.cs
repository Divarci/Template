using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using System.Net;
using Xunit;

namespace Core.Domain.Tests.Entities.Samples.Notes;

public sealed partial class NoteTests
{
    [Fact]
    public void Create_WithNullModel_ThrowsTemplateException()
    {
        // Arrange
        NoteCreateModel model = null!;

        // Act
        Action act = () => Note.Create(model);

        // Assert
        act.Should().Throw<TemplateException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceTitle_ReturnsValidationError(string? title)
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel(title: title!);

        // Act
        Result<Note> result = Note.Create(model);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Create_WithTooLongTitle_ReturnsValidationError()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel(title: CreateStringOfLength(TitleMaxLength + 1));

        // Act
        Result<Note> result = Note.Create(model);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Create_WithTooLongContent_ReturnsValidationError()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel(content: CreateStringOfLength(ContentMaxLength + 1));

        // Act
        Result<Note> result = Note.Create(model);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public void Create_WithSeveralInvalidFields_ReturnsOneErrorPerRule()
    {
        // Arrange
        NoteCreateModel model = CreateValidNoteCreateModel(
            title: string.Empty,
            content: CreateStringOfLength(ContentMaxLength + 1));

        // Act
        Result<Note> result = Note.Create(model);

        // Assert
        AssertFailureResult(result, HttpStatusCode.UnprocessableEntity);
        result.ErrorDetails!.Errors.Should().HaveCount(2);
    }
}
