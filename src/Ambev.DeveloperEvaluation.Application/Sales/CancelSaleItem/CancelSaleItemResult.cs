namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Result returned after cancelling an item in a sale.
/// </summary>
public class CancelSaleItemResult
{
    public Guid SaleId { get; set; }
    public Guid ItemId { get; set; }
    public decimal NewSaleTotalAmount { get; set; }
    public bool IsCancelled { get; set; }
}
