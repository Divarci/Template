using Core.Library.ResultPattern;
using System.Net;
using System.Text.RegularExpressions;

namespace Core.Library.Validators;

public static class StringValidator
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static Result Validate(
        this string? value,
        string fieldName,
        int maxLength,
        int minLength = 0,
        bool isRequired = true,
        bool isEmail = false)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure(
                message: $"{nameof(fieldName)} can not be null or empty",
                statusCode: HttpStatusCode.InternalServerError);

        List<string> validationErrors = [];

        if (isRequired && string.IsNullOrWhiteSpace(value))
            validationErrors.Add($"{fieldName} cannot be null or empty.");

        if (!string.IsNullOrWhiteSpace(value))
        {
            if (minLength > 0 && value.Length < minLength)
                validationErrors.Add($"{fieldName} must be at least {minLength} characters.");

            if (value.Length > maxLength)
                validationErrors.Add($"{fieldName} must be {maxLength} characters or less.");

            if (isEmail && !EmailRegex.IsMatch(value))
                validationErrors.Add($"{fieldName} must be a valid email address.");
        }

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
