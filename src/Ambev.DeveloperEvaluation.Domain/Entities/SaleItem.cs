using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents an individual product item within a sale.
/// Follows DDD principles with encapsulated business logic for discounts and totals.
/// </summary>
public class SaleItem : BaseEntity
{
    public const int MaxAllowedQuantity = 20;
    public const int MinQuantityForTenPercentDiscount = 4;
    public const int MinQuantityForTwentyPercentDiscount = 10;

    /// <summary>
    /// Gets or sets the sale ID this item belongs to.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Gets or sets the external product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized product description/name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the quantity of items purchased.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the unit price for the product.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Gets the discount percentage applied (e.g. 0.0m, 0.10m, 0.20m).
    /// </summary>
    public decimal DiscountPercentage { get; private set; }

    /// <summary>
    /// Gets the total monetary discount amount applied to this item.
    /// </summary>
    public decimal Discount { get; private set; }

    /// <summary>
    /// Gets the total monetary amount for this item after discount.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets whether this item has been cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Gets the date and time when the item was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the date and time when the item was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    protected SaleItem()
    {
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of SaleItem, validating business rules and calculating discount and total.
    /// </summary>
    /// <param name="productId">The external product ID.</param>
    /// <param name="productName">The denormalized product name.</param>
    /// <param name="quantity">The quantity purchased (1 to 20).</param>
    /// <param name="unitPrice">The unit price of the product.</param>
    public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        CreatedAt = DateTime.UtcNow;
        IsCancelled = false;

        SetPricingAndQuantity(quantity, unitPrice);
    }

    /// <summary>
    /// Updates the quantity and unit price, recalculating discounts and totals.
    /// </summary>
    /// <param name="quantity">The new quantity.</param>
    /// <param name="unitPrice">The new unit price.</param>
    public void Update(int quantity, decimal unitPrice)
    {
        if (IsCancelled)
            throw new DomainException("Cannot update a cancelled item.");

        SetPricingAndQuantity(quantity, unitPrice);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels this specific sale item.
    /// </summary>
    public void Cancel()
    {
        if (IsCancelled)
            return;

        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Enforces business rules for quantity, pricing, and discount tiers.
    /// </summary>
    private void SetPricingAndQuantity(int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException("Item quantity must be greater than zero.");

        if (quantity > MaxAllowedQuantity)
            throw new DomainException($"Cannot sell more than {MaxAllowedQuantity} identical items for product '{ProductName}'.");

        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        Quantity = quantity;
        UnitPrice = unitPrice;

        // Business rules for discount tiers:
        // - Purchases below 4 items cannot have a discount (0%)
        // - Purchases above 4 identical items (4-9) have a 10% discount
        // - Purchases between 10 and 20 identical items have a 20% discount
        if (quantity < MinQuantityForTenPercentDiscount)
        {
            DiscountPercentage = 0m;
        }
        else if (quantity < MinQuantityForTwentyPercentDiscount)
        {
            DiscountPercentage = 0.10m;
        }
        else
        {
            DiscountPercentage = 0.20m;
        }

        var subtotal = Quantity * UnitPrice;
        Discount = Math.Round(subtotal * DiscountPercentage, 2, MidpointRounding.AwayFromZero);
        TotalAmount = subtotal - Discount;
    }
}
