using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Event raised when an item in a sale is cancelled.
/// </summary>
public class ItemCancelledEvent : IDomainEvent
{
    /// <summary>
    /// Gets the sale aggregate containing the cancelled item.
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// Gets the cancelled item.
    /// </summary>
    public SaleItem Item { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of ItemCancelledEvent.
    /// </summary>
    /// <param name="sale">The sale aggregate.</param>
    /// <param name="item">The cancelled item.</param>
    public ItemCancelledEvent(Sale sale, SaleItem item)
    {
        Sale = sale;
        Item = item;
    }
}
