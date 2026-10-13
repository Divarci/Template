using Core.Library.ResultPattern;
using System.Net;

namespace Core.Library.Validators;

public static class DecimalValidator
{
    public static Result Validate(
        this decimal? value,
        string fieldName,
        bool isRequired = true,
        bool canBeNegative = false,
        decimal? minValue = null,
        decimal? maxValue = null)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure(
                message: $"{nameof(fieldName)} can not be null or empty",
                statusCode: HttpStatusCode.InternalServerError);

        if (minValue.HasValue && minValue.Value > decimal.MaxValue)
            return Result.Failure(
                message: $"{nameof(minValue)} cannot be greater than {decimal.MaxValue}",
                statusCode: HttpStatusCode.BadRequest);

        if (maxValue.HasValue && maxValue.Value > decimal.MaxValue)
            return Result.Failure(
                message: $"{nameof(maxValue)} cannot be greater than {decimal.MaxValue}",
                statusCode: HttpStatusCode.BadRequest);

        if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
            return Result.Failure(
                message: $"{nameof(minValue)} cannot be greater than {nameof(maxValue)}",
                statusCode: HttpStatusCode.BadRequest);

        List<string> validationErrors = [];

        if (isRequired && value is null)
            validationErrors.Add($"{fieldName} cannot be null");

        if (value is not null)
        {
            if (!canBeNegative && value < 0)
                validationErrors.Add($"{fieldName} cannot be negative");

            if (minValue.HasValue && value < minValue.Value)
                validationErrors.Add($"{fieldName} cannot be less than {minValue.Value}");

            if (maxValue.HasValue && value > maxValue.Value)
                validationErrors.Add($"{fieldName} cannot be greater than {maxValue.Value}");
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

    public static Result Validate(
        this decimal value,
        string fieldName,
        bool canBeNegative = false,
        decimal? minValue = null,
        decimal? maxValue = null)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure(
                message: $"{nameof(fieldName)} cannot be empty",
                statusCode: HttpStatusCode.InternalServerError);

        if (minValue.HasValue && minValue.Value > decimal.MaxValue)
            return Result.Failure(
                message: $"{nameof(minValue)} cannot be greater than {decimal.MaxValue}",
                statusCode: HttpStatusCode.BadRequest);

        if (maxValue.HasValue && maxValue.Value > decimal.MaxValue)
            return Result.Failure(
                message: $"{nameof(maxValue)} cannot be greater than {decimal.MaxValue}",
                statusCode: HttpStatusCode.BadRequest);

        if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
            return Result.Failure(
                message: $"{nameof(minValue)} cannot be greater than {nameof(maxValue)}",
                statusCode: HttpStatusCode.BadRequest);

        List<string> validationErrors = [];

        if (!canBeNegative && value < 0)
            validationErrors.Add($"{fieldName} cannot be negative");

        if (minValue.HasValue && value < minValue.Value)
            validationErrors.Add($"{fieldName} cannot be less than {minValue.Value}");

        if (maxValue.HasValue && value > maxValue.Value)
            validationErrors.Add($"{fieldName} cannot be greater than {maxValue.Value}");

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
