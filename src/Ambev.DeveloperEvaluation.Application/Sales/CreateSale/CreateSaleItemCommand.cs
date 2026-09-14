namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Command item representing a product to be included in the sale creation.
/// </summary>
public class CreateSaleItemCommand
{
    /// <summary>
    /// Gets or sets the external product identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized product description/name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity to purchase.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product.
    /// </summary>
    public decimal UnitPrice { get; set; }
}
