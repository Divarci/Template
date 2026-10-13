using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Services.System.OutboxMessages;

internal sealed partial class OutboxMessageService
{
    public async Task<Result<IReadOnlyList<OutboxMessage>>> GetUnprocessedMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
            return Result<IReadOnlyList<OutboxMessage>>.Failure(
                message: "Batch size must be greater than zero.",
                statusCode: HttpStatusCode.InternalServerError);

        return await _outboxMessageRepository
            .GetUnprocessedMessagesAsync(batchSize, cancellationToken);
    }
}
