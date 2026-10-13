using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Validators;

public static class EnumValidator
{
    public static Result Validate<TEnum>(
        this TEnum? value,
        string fieldName,
        bool isRequired = true) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure(
                message: $"{nameof(fieldName)} can not be null or empty",
                statusCode: HttpStatusCode.InternalServerError);

        List<string> validationErrors = [];

        if (isRequired && value is null)
            validationErrors.Add($"{fieldName} cannot be null");

        if (value.HasValue && !Enum.IsDefined(value.Value))
            validationErrors.Add($"{fieldName} has an invalid value");

        return validationErrors.Count > 0
            ? Result.Failure(
                message: $"{fieldName} validation failed",
                errorDetails: ErrorDetails.Create(validationErrors),
                statusCode: HttpStatusCode.UnprocessableEntity)
            : Result.Success(
                message: "Validation successful",
                statusCode: HttpStatusCode.OK);
    }

    public static Result Validate<TEnum>(
        this TEnum value,
        string fieldName) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure(
                message: $"{nameof(fieldName)} cannot be empty",
                statusCode: HttpStatusCode.InternalServerError);

        List<string> validationErrors = [];

        if (!Enum.IsDefined(value))
            validationErrors.Add($"{fieldName} has an invalid value");

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
