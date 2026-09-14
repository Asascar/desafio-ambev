using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Result returned after a sale is cancelled.
/// </summary>
public class CancelSaleResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public SaleStatus Status { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
