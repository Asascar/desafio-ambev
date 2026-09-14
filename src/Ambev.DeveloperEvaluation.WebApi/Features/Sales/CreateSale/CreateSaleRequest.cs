namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Request payload for creating an individual item within a sale.
/// </summary>
public class CreateSaleItemRequest
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
    /// Gets or sets the quantity of items to purchase (1-20).
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price for the product.
    /// </summary>
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// Request payload for creating a new sale.
/// </summary>
public class CreateSaleRequest
{
    /// <summary>
    /// Gets or sets an optional custom sale number. If omitted, will be auto-generated.
    /// </summary>
    public string? SaleNumber { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the sale was made. Defaults to current UTC time.
    /// </summary>
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the external customer identifier.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized customer name.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the external branch identifier.
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized branch name.
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of items in the sale.
    /// </summary>
    public List<CreateSaleItemRequest> Items { get; set; } = new();
}
