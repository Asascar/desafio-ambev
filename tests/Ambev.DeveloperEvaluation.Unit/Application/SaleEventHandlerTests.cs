using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for Sale Event Handlers and EventPublisher.
/// </summary>
public class SaleEventHandlerTests
{
    [Fact(DisplayName = "Given SaleCreatedNotification When handling Then logs information without error")]
    public async Task SaleCreatedEventHandler_ValidNotification_LogsSuccess()
    {
        // Given
        var logger = Substitute.For<ILogger<SaleCreatedEventHandler>>();
        var handler = new SaleCreatedEventHandler(logger);
        var sale = SaleTestData.GenerateValidSale(2);
        var notification = new SaleCreatedNotification(sale);

        // When
        var act = () => handler.Handle(notification, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Given SaleModifiedNotification When handling Then logs information without error")]
    public async Task SaleModifiedEventHandler_ValidNotification_LogsSuccess()
    {
        // Given
        var logger = Substitute.For<ILogger<SaleModifiedEventHandler>>();
        var handler = new SaleModifiedEventHandler(logger);
        var sale = SaleTestData.GenerateValidSale(1);
        var notification = new SaleModifiedNotification(sale);

        // When
        var act = () => handler.Handle(notification, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Given SaleCancelledNotification When handling Then logs information without error")]
    public async Task SaleCancelledEventHandler_ValidNotification_LogsSuccess()
    {
        // Given
        var logger = Substitute.For<ILogger<SaleCancelledEventHandler>>();
        var handler = new SaleCancelledEventHandler(logger);
        var sale = SaleTestData.GenerateValidSale(1);
        var notification = new SaleCancelledNotification(sale);

        // When
        var act = () => handler.Handle(notification, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Given ItemCancelledNotification When handling Then logs information without error")]
    public async Task ItemCancelledEventHandler_ValidNotification_LogsSuccess()
    {
        // Given
        var logger = Substitute.For<ILogger<ItemCancelledEventHandler>>();
        var handler = new ItemCancelledEventHandler(logger);
        var sale = SaleTestData.GenerateValidSale(1);
        var item = sale.Items.First();
        var notification = new ItemCancelledNotification(sale, item);

        // When
        var act = () => handler.Handle(notification, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Given domain event When publishing via EventPublisher Then sends MediatR notification")]
    public async Task EventPublisher_SaleCreatedEvent_PublishesMediatRNotification()
    {
        // Given
        var mediator = Substitute.For<IMediator>();
        var logger = Substitute.For<ILogger<EventPublisher>>();
        var publisher = new EventPublisher(mediator, logger);
        var sale = SaleTestData.GenerateValidSale(1);
        var domainEvent = new SaleCreatedEvent(sale);

        // When
        await publisher.PublishAsync(domainEvent, CancellationToken.None);

        // Then
        await mediator.Received(1).Publish(Arg.Any<SaleCreatedNotification>(), Arg.Any<CancellationToken>());
    }
}
