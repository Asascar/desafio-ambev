using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Event raised when a new sale is created.
/// </summary>
public class SaleCreatedEvent : IDomainEvent
{
    /// <summary>
    /// Gets the created sale aggregate.
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of SaleCreatedEvent.
    /// </summary>
    /// <param name="sale">The created sale.</param>
    public SaleCreatedEvent(Sale sale)
    {
        Sale = sale;
    }
}
