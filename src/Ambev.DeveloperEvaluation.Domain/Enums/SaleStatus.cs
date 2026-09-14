namespace Ambev.DeveloperEvaluation.Domain.Enums;

/// <summary>
/// Represents the current status of a sale.
/// </summary>
public enum SaleStatus
{
    /// <summary>
    /// The sale is active and confirmed.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The sale has been cancelled.
    /// </summary>
    Cancelled = 2
}
