using Core.Library.ResultPattern;
using Core.Library.Validators.Enums;
using System.Net;

namespace Core.Library.Validators;

public static class DateTimeValidator
{
    public static Result Validate(
        this DateTime? value,
        string fieldName,
        bool isRequired = true,
        DateTime? minValue = null,
        DateTime? maxValue = null,
        DateTimeRestriction restriction = DateTimeRestriction.None)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure(
                message: $"{nameof(fieldName)} can not be null or empty",
                statusCode: HttpStatusCode.InternalServerError);

        if (minValue.HasValue && minValue.Value > DateTime.MaxValue)
            return Result.Failure(
                message: $"{nameof(minValue)} cannot be greater than {DateTime.MaxValue}",
                statusCode: HttpStatusCode.BadRequest);

        if (maxValue.HasValue && maxValue.Value > DateTime.MaxValue)
            return Result.Failure(
                message: $"{nameof(maxValue)} cannot be greater than {DateTime.MaxValue}",
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
            if (restriction == DateTimeRestriction.OnlyPast && value > DateTime.UtcNow)
                validationErrors.Add($"{fieldName} cannot be a future date");

            if (restriction == DateTimeRestriction.OnlyFuture && value < DateTime.UtcNow)
                validationErrors.Add($"{fieldName} cannot be a past date");

            if (minValue.HasValue && value < minValue.Value)
                validationErrors.Add($"{fieldName} cannot be earlier than {minValue.Value:yyyy-MM-dd HH:mm:ss}");

            if (maxValue.HasValue && value > maxValue.Value)
                validationErrors.Add($"{fieldName} cannot be later than {maxValue.Value:yyyy-MM-dd HH:mm:ss}");
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
        this DateTime value,
        string fieldName,
        DateTime? minValue = null,
        DateTime? maxValue = null,
        DateTimeRestriction restriction = DateTimeRestriction.None)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Result.Failure(
                message: $"{nameof(fieldName)} cannot be empty",
                statusCode: HttpStatusCode.InternalServerError);

        if (minValue.HasValue && minValue.Value > DateTime.MaxValue)
            return Result.Failure(
                message: $"{nameof(minValue)} cannot be greater than {DateTime.MaxValue}",
                statusCode: HttpStatusCode.BadRequest);

        if (maxValue.HasValue && maxValue.Value > DateTime.MaxValue)
            return Result.Failure(
                message: $"{nameof(maxValue)} cannot be greater than {DateTime.MaxValue}",
                statusCode: HttpStatusCode.BadRequest);

        if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
            return Result.Failure(
                message: $"{nameof(minValue)} cannot be greater than {nameof(maxValue)}",
                statusCode: HttpStatusCode.BadRequest);

        List<string> validationErrors = [];

        if (restriction == DateTimeRestriction.OnlyPast && value > DateTime.UtcNow)
            validationErrors.Add($"{fieldName} cannot be a future date");

        if (restriction == DateTimeRestriction.OnlyFuture && value < DateTime.UtcNow)
            validationErrors.Add($"{fieldName} cannot be a past date");

        if (minValue.HasValue && value < minValue.Value)
            validationErrors.Add($"{fieldName} cannot be earlier than {minValue.Value:yyyy-MM-dd HH:mm:ss}");

        if (maxValue.HasValue && value > maxValue.Value)
            validationErrors.Add($"{fieldName} cannot be later than {maxValue.Value:yyyy-MM-dd HH:mm:ss}");

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
