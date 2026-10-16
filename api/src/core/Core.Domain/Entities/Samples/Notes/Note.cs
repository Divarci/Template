using Core.Domain.Entities.Samples.Notes.DomainEvents;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Library.Abstractions;
using Core.Library.Abstractions.Interfaces;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Domain.Entities.Samples.Notes;

public sealed partial class Note : BaseEntity, IHaveSoftDelete
{
    private Note() { }

    private Note(Guid id, string title, string? content, bool isPinned)
    {
        Id = id;
        Title = title;
        Content = content;
        IsPinned = isPinned;
    }

    public string Title { get; private set; } = default!;
    public string? Content { get; private set; }
    public bool IsPinned { get; private set; }
    public bool IsDeleted { get; private set; }

    public static Result<Note> Create(NoteCreateModel createModel)
    {
        if (createModel is null)
            throw new TemplateException(
                assemblyName: "Core.Domain",
                className: nameof(Note),
                methodName: nameof(Create),
                message: "Create model cannot be null.");

        Note note = new(
            id: Guid.NewGuid(),
            title: createModel.Title,
            content: createModel.Content,
            isPinned: createModel.IsPinned);

        Result validationResult = Validate(note);

        if (validationResult.IsFailure)
            return Result<Note>.Failure(
                message: validationResult.Message,
                errorDetails: validationResult.ErrorDetails!,
                statusCode: validationResult.StatusCode);

        note.Raise(new NoteCreatedDomainEvent(note.Id));

        return Result<Note>.Success(
            message: "Note created successfully",
            data: note,
            statusCode: HttpStatusCode.Created);
    }

    public Result UpdateTitle(string title)
    {
        if (IsDeleted)
            return Result.Failure(
                message: "Note is already deleted.",
                statusCode: HttpStatusCode.Conflict);

        Title = title;

        return ValidateState("Note title updated successfully");
    }

    public Result UpdateContent(string content)
    {
        if (IsDeleted)
            return Result.Failure(
                message: "Note is already deleted.",
                statusCode: HttpStatusCode.Conflict);

        Content = content;

        return ValidateState("Note content updated successfully");
    }

    public Result UpdatePinned(bool isPinned)
    {
        if (IsDeleted)
            return Result.Failure(
                message: "Note is already deleted.",
                statusCode: HttpStatusCode.Conflict);

        IsPinned = isPinned;

        return ValidateState("Note pin state updated successfully");
    }

    public Result SoftDelete()
    {
        if (IsDeleted)
            return Result.Failure(
                message: "Note is already deleted.",
                statusCode: HttpStatusCode.Conflict);

        IsDeleted = true;

        return Result.Success(
            message: "Note soft-deleted successfully",
            statusCode: HttpStatusCode.OK);
    }
}
