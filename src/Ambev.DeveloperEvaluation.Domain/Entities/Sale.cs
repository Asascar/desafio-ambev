using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Aggregate Root representing a sale record in the DeveloperStore.
/// Follows DDD principles, encapsulating external identities, items, discount rules, and domain events.
/// </summary>
public class Sale : BaseEntity
{
    private readonly List<SaleItem> _items = new();
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets or sets the business identification number of the sale.
    /// </summary>
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the sale was made.
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Gets or sets the external customer identifier.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized customer name/description.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the external branch identifier.
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized branch name/description.
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the total monetary amount of the sale (sum of active items).
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets the status of the sale (Active or Cancelled).
    /// </summary>
    public SaleStatus Status { get; private set; }

    /// <summary>
    /// Helper property indicating whether the sale is cancelled.
    /// </summary>
    public bool IsCancelled => Status == SaleStatus.Cancelled;

    /// <summary>
    /// Gets the collection of items included in this sale.
    /// </summary>
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Gets domain events registered on this aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Gets the date and time when the sale was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the date and time when the sale was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Parameterless constructor for EF Core.
    /// </summary>
    protected Sale()
    {
        CreatedAt = DateTime.UtcNow;
        Status = SaleStatus.Active;
    }

    /// <summary>
    /// Initializes a new instance of Sale.
    /// </summary>
    public Sale(
        string saleNumber,
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("Sale number cannot be empty.");

        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID is required.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name cannot be empty.");

        if (branchId == Guid.Empty)
            throw new DomainException("Branch ID is required.");

        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("Branch name cannot be empty.");

        Id = Guid.NewGuid();
        SaleNumber = saleNumber;
        SaleDate = saleDate == default ? DateTime.UtcNow : saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        Status = SaleStatus.Active;
        CreatedAt = DateTime.UtcNow;
        TotalAmount = 0m;

        AddDomainEvent(new SaleCreatedEvent(this));
    }

    /// <summary>
    /// Adds an item to the sale or updates quantity if identical product already exists in the sale.
    /// Enforces the maximum limit of 20 items per product.
    /// </summary>
    public SaleItem AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (IsCancelled)
            throw new DomainException("Cannot add items to a cancelled sale.");

        if (productId == Guid.Empty)
            throw new DomainException("Product ID is required.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name cannot be empty.");

        var existingActiveItem = _items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        if (existingActiveItem != null)
        {
            var combinedQuantity = existingActiveItem.Quantity + quantity;
            if (combinedQuantity > SaleItem.MaxAllowedQuantity)
                throw new DomainException($"Total quantity for product '{productName}' cannot exceed {SaleItem.MaxAllowedQuantity}. Current: {existingActiveItem.Quantity}, Attempted addition: {quantity}.");

            existingActiveItem.Update(combinedQuantity, unitPrice);
            RecalculateTotals();
            return existingActiveItem;
        }

        var item = new SaleItem(productId, productName, quantity, unitPrice)
        {
            SaleId = Id
        };

        _items.Add(item);
        RecalculateTotals();
        return item;
    }

    /// <summary>
    /// Cancels a specific item within the sale and recalculates totals.
    /// Raises ItemCancelledEvent and SaleModifiedEvent.
    /// </summary>
    public void CancelItem(Guid itemId)
    {
        if (IsCancelled)
            throw new DomainException("Sale is already cancelled.");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new DomainException($"Item with ID '{itemId}' not found in this sale.");

        if (item.IsCancelled)
            return;

        item.Cancel();
        RecalculateTotals();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ItemCancelledEvent(this, item));
        AddDomainEvent(new SaleModifiedEvent(this));
    }

    /// <summary>
    /// Cancels the entire sale, cancels all active items, and raises SaleCancelledEvent.
    /// </summary>
    public void Cancel()
    {
        if (IsCancelled)
            return;

        Status = SaleStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        foreach (var item in _items.Where(i => !i.IsCancelled))
        {
            item.Cancel();
        }

        RecalculateTotals();
        AddDomainEvent(new SaleCancelledEvent(this));
    }

    /// <summary>
    /// Updates sale header details.
    /// </summary>
    public void UpdateDetails(DateTime saleDate, Guid customerId, string customerName, Guid branchId, string branchName)
    {
        if (IsCancelled)
            throw new DomainException("Cannot update details of a cancelled sale.");

        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID is required.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name cannot be empty.");

        if (branchId == Guid.Empty)
            throw new DomainException("Branch ID is required.");

        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("Branch name cannot be empty.");

        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SaleModifiedEvent(this));
    }

    /// <summary>
    /// Recalculates the total amount of the sale based on active items.
    /// </summary>
    public void RecalculateTotals()
    {
        TotalAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
    }

    /// <summary>
    /// Registers a domain event.
    /// </summary>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the aggregate.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
