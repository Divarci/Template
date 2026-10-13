using Core.Domain.Entities.System.OutboxMessages;
using Core.Domain.Entities.System.OutboxMessages.Interfaces;
using Core.Library.Contracts.DomainEvents.Handle;
using Core.Library.Contracts.DomainEvents.Process;
using Core.Library.Exceptions;
using Core.Library.ResultPattern;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace App.Processor.DomainEvents;

internal sealed partial class OutboxProcessor
{
    private static readonly ConcurrentDictionary<Type, Type[]> HandlersDictionary = new();

    private static IEnumerable<IDomainEventHandler> GetHandlers(
        Type type,
        IServiceProvider serviceProvider)
    {
        Type[] domainEventHandlerTypes = HandlersDictionary.GetOrAdd(
            type,
            _ => [.. ServiceRegistrar.Assembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler<>).MakeGenericType(type)))]);

        List<IDomainEventHandler> handlers = [];

        foreach (Type domainEventHandlerType in domainEventHandlerTypes)
        {
            object domainEventHandler = serviceProvider.GetRequiredService(domainEventHandlerType);

            handlers.Add((domainEventHandler as IDomainEventHandler)!);
        }

        return handlers;
    }

    private static async Task InitialiseHandlersAsync(
        OutboxMessage outboxMessage,
        JsonSerializerOptions jsonSerializerOptions,
        IServiceScopeFactory serviceScopeFactory,
        CancellationToken cancellationToken = default)
    {
        Type? domainEventType = Type.GetType(outboxMessage.Type) ??
            throw new TemplateException(
                assemblyName: "App.Processor",
                className: nameof(OutboxProcessor),
                methodName: nameof(InitialiseHandlersAsync),
                message: $"Unable to resolve type: {outboxMessage.Type}");

        IDomainEvent domainEvent = (IDomainEvent)JsonSerializer.Deserialize(
            outboxMessage.Content, domainEventType, jsonSerializerOptions)!;

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        IEnumerable<IDomainEventHandler> handlers = GetHandlers(
            domainEvent.GetType(),
            scope.ServiceProvider);

        foreach (IDomainEventHandler domainEventHandler in handlers)
            await domainEventHandler.Handle(domainEvent, cancellationToken);
    }

    private static async Task<Result> UpdateAsync(
        IOutboxMessageService outboxMessageService,
        OutboxMessage outboxMessage,
        Exception? exception)
    {
        Result<OutboxMessage> messageResult = await outboxMessageService
            .GetByIdAsync(outboxMessage.Id);

        if (messageResult.IsFailureAndNoData)
            return Result.Failure(
                message: messageResult.Message,
                statusCode: messageResult.StatusCode);

        messageResult.Data.Update(exception?.ToString());

        return Result.Success(
            message: "Outbox updated successfully",
            statusCode: HttpStatusCode.OK);
    }
}
