using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class SaleCreatedNotification : INotification
{
    public Sale Sale { get; }

    public SaleCreatedNotification(Sale sale)
    {
        Sale = sale;
    }
}

public class SaleModifiedNotification : INotification
{
    public Sale Sale { get; }

    public SaleModifiedNotification(Sale sale)
    {
        Sale = sale;
    }
}

public class SaleCancelledNotification : INotification
{
    public Sale Sale { get; }

    public SaleCancelledNotification(Sale sale)
    {
        Sale = sale;
    }
}

public class ItemCancelledNotification : INotification
{
    public Sale Sale { get; }
    public SaleItem Item { get; }

    public ItemCancelledNotification(Sale sale, SaleItem item)
    {
        Sale = sale;
        Item = item;
    }
}
