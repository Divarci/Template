using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.ResultPattern;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infra.Persistence.Repositories.System.OutboxMessages;

internal sealed partial class OutboxMessageRepository
{
    public async Task<Result<IReadOnlyList<OutboxMessage>>> GetUnprocessedMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        OutboxMessage[] messages = await _context
            .Set<OutboxMessage>()
            .Where(x => !x.ProcessedOnUtc.HasValue)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);

        return Result<IReadOnlyList<OutboxMessage>>.Success(
            message: "Unprocessed messages retrieved successfully",
            data: messages,
            statusCode: HttpStatusCode.OK);
    }
}
