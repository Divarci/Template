using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Services.System.OutboxMessages;

internal sealed partial class OutboxMessageService
{
    public Result MarkProcessed(
        OutboxMessage message,
        string? error)
    {
        if (message is null)
            throw new TemplateException(
                assemblyName: "Core.Services",
                className: nameof(OutboxMessageService),
                methodName: nameof(MarkProcessed),
                message: "Outbox message cannot be null.");

        Result markProcessedResult = message.MarkProcessed(error);

        if (markProcessedResult.IsFailure)
            return markProcessedResult;

        Result updateResult = _outboxMessageRepository.Update(message);

        if (updateResult.IsFailure)
            return updateResult;

        return Result.Success(
            message: markProcessedResult.Message,
            statusCode: HttpStatusCode.NoContent);
    }
}
