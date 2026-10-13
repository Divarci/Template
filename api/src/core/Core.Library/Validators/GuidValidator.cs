using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Validators;

public static class GuidValidator
{
    public static Result Validate(
        this Guid? value,
        string fieldName,
        bool isRequired = true,
        bool allowEmpty = true)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure(
                message: $"{nameof(fieldName)} can not be null or empty",
                statusCode: HttpStatusCode.InternalServerError);

        List<string> validationErrors = [];

        if (isRequired && value is null)
            validationErrors.Add($"{fieldName} cannot be null");

        if (value is not null && !allowEmpty && value == Guid.Empty)
            validationErrors.Add($"{fieldName} cannot be empty guid");

        return validationErrors.Count > 0
            ? Result.Failure(
                message: $"{fieldName} validation failed",
                errorDetails: ErrorDetails.Create(validationErrors),
                statusCode: HttpStatusCode.UnprocessableEntity)
            : Result.Success(
                message: "Validation successful",
                statusCode: HttpStatusCode.OK);
    }

    public static Result Validate(
        this Guid value,
        string fieldName,
        bool allowEmpty = true)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure(
                message: $"{nameof(fieldName)} cannot be empty",
                statusCode: HttpStatusCode.InternalServerError);

        List<string> validationErrors = [];

        if (!allowEmpty && value == Guid.Empty)
            validationErrors.Add($"{fieldName} cannot be empty guid");

        return validationErrors.Count > 0
            ? Result.Failure(
                message: $"{fieldName} validation failed",
                errorDetails: ErrorDetails.Create(validationErrors),
                statusCode: HttpStatusCode.UnprocessableEntity)
            : Result.Success(
                message: "Validation successful",
                statusCode: HttpStatusCode.OK);
    }
}
