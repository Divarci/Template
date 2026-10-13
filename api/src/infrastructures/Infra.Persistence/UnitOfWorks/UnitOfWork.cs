using Core.Domain.Entities.System.OutboxMessages;
using Core.Library.Abstractions;
using Core.Library.Contracts.DomainEvents.Process;
using Core.Library.Contracts.UnitOfWorks;
using Core.Library.ResultPattern;
using Infra.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Infra.Persistence.UnitOfWorks;

internal sealed class UnitOfWork(
    AppDbContext context,
    ILogger<UnitOfWork> logger) : ITransactionalUnitOfWork
{
    public async Task<Result> ExecuteTransactionAsync(
        Func<Task<Result>> operation,
        CancellationToken cancellationToken = default)
    {
        await using IDbContextTransaction transaction = await context.Database
            .BeginTransactionAsync(cancellationToken);

        string traceId = Guid.NewGuid().ToString();

        try
        {
            logger.LogDebug("Transaction started.");

            Result result = await operation();

            if (result.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);

                logger.LogWarning(
                    "Transaction rolled back. Operation returned a failure result. {Message} | TraceId: {TraceId}",
                    result.Message,
                    traceId);

                return result;
            }

            Result commitResult = await CommitAsync(transaction, cancellationToken);

            if (commitResult.IsFailure)
                return commitResult;

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            logger.LogError(ex, "Transaction rolled back due to an unhandled exception. TraceId: {TraceId}.", traceId);

            return Result.Failure(
                message: ex.Message,
                statusCode: HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<T>> ExecuteTransactionAsync<T>(
        Func<Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        await using IDbContextTransaction transaction = await context.Database
            .BeginTransactionAsync(cancellationToken);

        string traceId = Guid.NewGuid().ToString();

        try
        {
            logger.LogDebug("Transaction started.");

            Result<T> result = await operation();

            if (result.IsFailureAndNoData)
            {
                await transaction.RollbackAsync(cancellationToken);

                logger.LogWarning(
                    "Transaction rolled back. Operation returned a failure result. {Message} | TraceId: {TraceId}",
                    result.Message,
                    traceId);

                return result;
            }

            Result commitResult = await CommitAsync(transaction, cancellationToken);

            if (commitResult.IsFailure)
                return Result<T>.Failure(
                    message: commitResult.Message,
                    statusCode: commitResult.StatusCode);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            logger.LogError(ex, "Transaction rolled back due to an unhandled exception. TraceId: {TraceId}.", traceId);

            return Result<T>.Failure(
                message: ex.Message,
                statusCode: HttpStatusCode.InternalServerError);
        }
    }

    private async Task<Result> CommitAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        string traceId = Guid.NewGuid().ToString();

        Result domainEventsRegisterResult = await AddDomainEventsAsOutboxMessages(context);

        if (domainEventsRegisterResult.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);

            logger.LogError(
                "Transaction rolled back. Failed to register domain events. {Message} | TraceId: {TraceId}",
                domainEventsRegisterResult.Message,
                traceId);

            return domainEventsRegisterResult;
        }

        await context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        logger.LogDebug("Transaction committed successfully.");

        return Result.Success(
            message: "Transaction committed.",
            statusCode: HttpStatusCode.OK);
    }

    private static async Task<Result> AddDomainEventsAsOutboxMessages(DbContext dbContext)
    {
        List<Result<OutboxMessage>> outboxMessageResults = [.. dbContext.ChangeTracker
            .Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = [.. entity.DomainEvents];

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .Select(domainEvent => OutboxMessage.Create(
                Guid.NewGuid(),
                domainEvent.GetType().AssemblyQualifiedName!,
                JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions.Instance),
                DateTime.UtcNow))];

        if (outboxMessageResults.Any(x => x.IsFailure))
            return Result.Failure(
                message: "Failed to create outbox messages from domain events.",
                statusCode: HttpStatusCode.InternalServerError);

        await dbContext.AddRangeAsync(outboxMessageResults.Select(x => x.Data)!);

        return Result.Success(
            message: "Outbox messages created successfully",
            statusCode: HttpStatusCode.OK);
    }
}
