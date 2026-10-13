using Core.Library.Abstractions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Domain.Entities.System.OutboxMessages;

public sealed partial class OutboxMessage : BaseEntity
{
    private OutboxMessage() { }

    private OutboxMessage(Guid id, string type,
        string content, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }

    public string Type { get; private set; } = default!;

    public string Content { get; private set; } = default!;

    public DateTime OccurredOnUtc { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public static Result<OutboxMessage> Create(Guid id,
        string type, string content, DateTime occurredOnUtc)
    {
        OutboxMessage message = new(id, type, content, occurredOnUtc);

        Result validationResult = Validate(message);

        if (validationResult.IsFailure)
            return Result<OutboxMessage>.Failure(
                message: validationResult.Message,
                errorDetails: validationResult.ErrorDetails!,
                statusCode: validationResult.StatusCode);

        return Result<OutboxMessage>.Success(
            message: "Outbox message created successfully",
            data: message,
            statusCode: HttpStatusCode.Created);
    }

    public Result Update(string? exception)
    {
        ProcessedOnUtc = DateTime.UtcNow;
        Error = exception;

        Result validationResult = Validate(this);

        if (validationResult.IsFailure)
            return Result.Failure(
                message: validationResult.Message,
                errorDetails: validationResult.ErrorDetails!,
                statusCode: validationResult.StatusCode);

        return Result.Success(
            message: "Outbox message updated successfully",
            statusCode: HttpStatusCode.OK);
    }
}
