using Core.Library.ResultPattern;
using Core.Library.Validators;
using System.Net;

namespace Core.Domain.Entities.System.OutboxMessages;

public sealed partial class OutboxMessage
{
    private static Result Validate(OutboxMessage message)
    {
        List<Result> results = [];

        ValidateProperties(results, message);
        ValidateBusiness(results, message);

        return Result.CombineValidationErrors(results);
    }

    private static void ValidateProperties(List<Result> results, OutboxMessage message)
    {
        results.AddRange(
            [
                message.Type.Validate(
                    fieldName: nameof(message.Type),
                    maxLength: 2000),
                message.Content.Validate(
                    fieldName: nameof(message.Content),
                    maxLength: 100000),
                message.OccurredOnUtc.Validate(
                    fieldName: nameof(message.OccurredOnUtc))
            ]
        );
    }

    private static void ValidateBusiness(List<Result> results, OutboxMessage message)
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
