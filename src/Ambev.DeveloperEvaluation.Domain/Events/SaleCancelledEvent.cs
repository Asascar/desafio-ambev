using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Event raised when a sale is cancelled.
/// </summary>
public class SaleCancelledEvent : IDomainEvent
{
    /// <summary>
    /// Gets the cancelled sale aggregate.
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of SaleCancelledEvent.
    /// </summary>
    /// <param name="sale">The cancelled sale.</param>
    public SaleCancelledEvent(Sale sale)
    {
        Sale = sale;
    }
}
