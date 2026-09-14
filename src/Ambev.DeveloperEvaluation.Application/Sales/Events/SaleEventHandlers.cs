using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Notification handler for SaleCreatedNotification.
/// </summary>
public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedNotification>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DOMAIN EVENT: SaleCreated] Sale {SaleNumber} (ID: {SaleId}) created for Customer {CustomerName} ({CustomerId}) at Branch {BranchName} with {ItemCount} items. Total: {TotalAmount:C}",
            notification.Sale.SaleNumber,
            notification.Sale.Id,
            notification.Sale.CustomerName,
            notification.Sale.CustomerId,
            notification.Sale.BranchName,
            notification.Sale.Items.Count,
            notification.Sale.TotalAmount);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Notification handler for SaleModifiedNotification.
/// </summary>
public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedNotification>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleModifiedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DOMAIN EVENT: SaleModified] Sale {SaleNumber} (ID: {SaleId}) was modified. New Total: {TotalAmount:C}, Status: {Status}",
            notification.Sale.SaleNumber,
            notification.Sale.Id,
            notification.Sale.TotalAmount,
            notification.Sale.Status);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Notification handler for SaleCancelledNotification.
/// </summary>
public class SaleCancelledEventHandler : INotificationHandler<SaleCancelledNotification>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCancelledNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DOMAIN EVENT: SaleCancelled] Sale {SaleNumber} (ID: {SaleId}) has been cancelled.",
            notification.Sale.SaleNumber,
            notification.Sale.Id);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Notification handler for ItemCancelledNotification.
/// </summary>
public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledNotification>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ItemCancelledNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DOMAIN EVENT: ItemCancelled] Item {ItemId} ({ProductName}, ProductId: {ProductId}) was cancelled in Sale {SaleNumber}. New Sale Total: {TotalAmount:C}",
            notification.Item.Id,
            notification.Item.ProductName,
            notification.Item.ProductId,
            notification.Sale.SaleNumber,
            notification.Sale.TotalAmount);

        return Task.CompletedTask;
    }
}
