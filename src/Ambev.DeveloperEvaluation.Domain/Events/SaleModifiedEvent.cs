using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Event raised when a sale is modified.
/// </summary>
public class SaleModifiedEvent : IDomainEvent
{
    /// <summary>
    /// Gets the modified sale aggregate.
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of SaleModifiedEvent.
    /// </summary>
    /// <param name="sale">The modified sale.</param>
    public SaleModifiedEvent(Sale sale)
    {
        Sale = sale;
    }
}
