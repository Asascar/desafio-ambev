using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Default implementation of IEventPublisher using MediatR and ILogger.
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly IMediator _mediator;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(IMediator mediator, ILogger<EventPublisher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        _logger.LogInformation("Publishing domain event: {EventType} occurred at {OccurredOn}",
            domainEvent.GetType().Name, domainEvent.OccurredOn);

        switch (domainEvent)
        {
            case SaleCreatedEvent createdEvent:
                await _mediator.Publish(new SaleCreatedNotification(createdEvent.Sale), cancellationToken);
                break;

            case SaleModifiedEvent modifiedEvent:
                await _mediator.Publish(new SaleModifiedNotification(modifiedEvent.Sale), cancellationToken);
                break;

            case SaleCancelledEvent cancelledEvent:
                await _mediator.Publish(new SaleCancelledNotification(cancelledEvent.Sale), cancellationToken);
                break;

            case ItemCancelledEvent itemCancelledEvent:
                await _mediator.Publish(new ItemCancelledNotification(itemCancelledEvent.Sale, itemCancelledEvent.Item), cancellationToken);
                break;

            default:
                _logger.LogWarning("No notification mapping registered for domain event {EventType}", domainEvent.GetType().Name);
                break;
        }
    }
}
