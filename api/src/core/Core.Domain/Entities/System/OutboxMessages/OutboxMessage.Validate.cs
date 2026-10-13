using Core.Library.ResultPattern;
using Core.Library.Validators;

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
                    nameof(message.Type),
                    maxLength: 2000),
                message.Content.Validate(
                    nameof(message.Content),
                    maxLength: 100000),
                message.OccurredOnUtc.Validate(
                    nameof(message.OccurredOnUtc))
            ]
        );
    }

    private static void ValidateBusiness(List<Result> results, OutboxMessage message)
    {
    }
}
