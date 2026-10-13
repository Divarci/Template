using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Validators;

public static class ValueObjectValidator
{
    public static Result Validate<TValueObject>(
        this TValueObject? value,
        string fieldName) where TValueObject : class
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure(
                message: $"{nameof(fieldName)} cannot be empty",
                statusCode: HttpStatusCode.InternalServerError);

        if (value is null)
            return Result.Failure(
                message: $"{fieldName} validation failed",
                errorDetails: ErrorDetails.Create([$"{fieldName} cannot be null"]),
                statusCode: HttpStatusCode.UnprocessableEntity);

        return Result.Success(
                message: "Validation successful",
                statusCode: HttpStatusCode.OK);
    }
}
