using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;

namespace Core.Domain.Entities.Samples.Notes;

public sealed partial class Note
{
    private static Result Validate(Note note)
    {
        List<Result> results = [];

        ValidateProperties(results, note);
        ValidateBusiness(results, note);

        return Result.CombineValidationErrors(results);
    }

    private static void ValidateProperties(List<Result> results, Note note)
    {
        results.AddRange(
            [
                note.Title.Validate(
                    fieldName: nameof(note.Title),
                    maxLength: 200),
                note.Content.Validate(
                    fieldName: nameof(note.Content),
                    maxLength: 5000,
                    isRequired: false)
            ]
        );
    }

    private static void ValidateBusiness(List<Result> results, Note note)
    {
    }

    private Result ValidateState(string successMessage)
    {
        Result validationResult = Validate(this);

        if (validationResult.IsFailure)
            return Result.Failure(
                message: validationResult.Message,
                errorDetails: validationResult.ErrorDetails!,
                statusCode: validationResult.StatusCode);

        return Result.Success(
            message: successMessage,
            statusCode: HttpStatusCode.OK);
    }
}
