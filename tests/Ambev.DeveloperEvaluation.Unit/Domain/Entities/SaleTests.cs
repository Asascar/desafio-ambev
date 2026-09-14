using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Sale"/> aggregate root entity.
/// </summary>
public class SaleTests
{
    [Fact(DisplayName = "Given valid parameters When creating sale Then initializes with Active status and SaleCreatedEvent")]
    public void Constructor_ValidParameters_InitializesCorrectly()
    {
        // Given
        var saleNumber = "SALE-001";
        var saleDate = DateTime.UtcNow;
        var customerId = Guid.NewGuid();
        var customerName = "John Doe";
        var branchId = Guid.NewGuid();
        var branchName = "Main Branch";

        // When
        var sale = new Sale(saleNumber, saleDate, customerId, customerName, branchId, branchName);

        // Then
        sale.SaleNumber.Should().Be(saleNumber);
        sale.CustomerId.Should().Be(customerId);
        sale.CustomerName.Should().Be(customerName);
        sale.BranchId.Should().Be(branchId);
        sale.BranchName.Should().Be(branchName);
        sale.Status.Should().Be(SaleStatus.Active);
        sale.IsCancelled.Should().BeFalse();
        sale.TotalAmount.Should().Be(0m);
        sale.Items.Should().BeEmpty();
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCreatedEvent);
    }

    [Theory(DisplayName = "Given missing mandatory fields When creating sale Then throws DomainException")]
    [InlineData("", "Customer", "Branch")]
    [InlineData("SALE-1", "", "Branch")]
    [InlineData("SALE-1", "Customer", "")]
    public void Constructor_MissingFields_ThrowsDomainException(string saleNumber, string customerName, string branchName)
    {
        // When
        var act = () => new Sale(
            saleNumber,
            DateTime.UtcNow,
            customerName == "" ? Guid.Empty : Guid.NewGuid(),
            customerName,
            branchName == "" ? Guid.Empty : Guid.NewGuid(),
            branchName);

        // Then
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Given active sale When adding items Then updates items and recalculates total")]
    public void AddItem_ValidItems_AddsAndRecalculatesTotal()
    {
        // Given
        var sale = new Sale("SALE-002", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        // When: Item 1: 5 * 20 = 100 with 10% discount (10) = 90
        sale.AddItem(productId1, "Product 1", 5, 20m);
        // When: Item 2: 2 * 50 = 100 with 0% discount = 100
        sale.AddItem(productId2, "Product 2", 2, 50m);

        // Then
        sale.Items.Should().HaveCount(2);
        sale.TotalAmount.Should().Be(190m);
    }

    [Fact(DisplayName = "Given active sale When adding identical product Then increases quantity and recalculates discount")]
    public void AddItem_IdenticalProduct_UpdatesQuantityAndRecalculates()
    {
        // Given
        var sale = new Sale("SALE-003", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
        var productId = Guid.NewGuid();

        // When: Add 3 items initially (no discount: 3 * 10 = 30)
        sale.AddItem(productId, "Product A", 3, 10m);
        sale.TotalAmount.Should().Be(30m);

        // When: Add 2 more items of the same product -> total quantity becomes 5 (10% discount on 50 = 45)
        sale.AddItem(productId, "Product A", 2, 10m);

        // Then
        sale.Items.Should().HaveCount(1);
        var item = sale.Items.First();
        item.Quantity.Should().Be(5);
        item.DiscountPercentage.Should().Be(0.10m);
        item.TotalAmount.Should().Be(45m);
        sale.TotalAmount.Should().Be(45m);
    }

    [Fact(DisplayName = "Given identical product When adding quantity that exceeds 20 items Then throws DomainException")]
    public void AddItem_IdenticalProductExceedsTwenty_ThrowsDomainException()
    {
        // Given
        var sale = new Sale("SALE-004", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Product X", 15, 10m);

        // When
        var act = () => sale.AddItem(productId, "Product X", 6, 10m); // 15 + 6 = 21 > 20

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot exceed 20*");
    }

    [Fact(DisplayName = "Given cancelled sale When adding item Then throws DomainException")]
    public void AddItem_CancelledSale_ThrowsDomainException()
    {
        // Given
        var sale = new Sale("SALE-005", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
        sale.Cancel();

        // When
        var act = () => sale.AddItem(Guid.NewGuid(), "Product Y", 2, 10m);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot add items to a cancelled sale*");
    }

    [Fact(DisplayName = "Given sale with items When cancelling single item Then recalculates total and registers events")]
    public void CancelItem_ExistingItem_CancelsAndRecalculatesTotal()
    {
        // Given
        var sale = new Sale("SALE-006", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
        var item1 = sale.AddItem(Guid.NewGuid(), "Item 1", 5, 20m); // 90
        var item2 = sale.AddItem(Guid.NewGuid(), "Item 2", 2, 50m); // 100
        sale.TotalAmount.Should().Be(190m);

        // When
        sale.CancelItem(item1.Id);

        // Then
        item1.IsCancelled.Should().BeTrue();
        item2.IsCancelled.Should().BeFalse();
        sale.TotalAmount.Should().Be(100m);
        sale.DomainEvents.Should().Contain(e => e is ItemCancelledEvent);
        sale.DomainEvents.Should().Contain(e => e is SaleModifiedEvent);
    }

    [Fact(DisplayName = "Given sale with items When cancelling entire sale Then sets Cancelled status, cancels items, and registers SaleCancelledEvent")]
    public void Cancel_ActiveSale_CancelsSaleAndAllItems()
    {
        // Given
        var sale = new Sale("SALE-007", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
        var item1 = sale.AddItem(Guid.NewGuid(), "Item 1", 5, 20m);
        var item2 = sale.AddItem(Guid.NewGuid(), "Item 2", 2, 50m);

        // When
        sale.Cancel();

        // Then
        sale.Status.Should().Be(SaleStatus.Cancelled);
        sale.IsCancelled.Should().BeTrue();
        item1.IsCancelled.Should().BeTrue();
        item2.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(0m);
        sale.DomainEvents.Should().Contain(e => e is SaleCancelledEvent);
    }

    [Fact(DisplayName = "Given active sale When updating details Then updates properties and registers SaleModifiedEvent")]
    public void UpdateDetails_ValidDetails_UpdatesAndRegistersEvent()
    {
        // Given
        var sale = new Sale("SALE-008", DateTime.UtcNow, Guid.NewGuid(), "Customer 1", Guid.NewGuid(), "Branch 1");
        var newDate = DateTime.UtcNow.AddDays(-1);
        var newCustId = Guid.NewGuid();
        var newBranchId = Guid.NewGuid();

        // When
        sale.UpdateDetails(newDate, newCustId, "Customer 2", newBranchId, "Branch 2");

        // Then
        sale.SaleDate.Should().Be(newDate);
        sale.CustomerId.Should().Be(newCustId);
        sale.CustomerName.Should().Be("Customer 2");
        sale.BranchId.Should().Be(newBranchId);
        sale.BranchName.Should().Be("Branch 2");
        sale.DomainEvents.Should().Contain(e => e is SaleModifiedEvent);
    }
}
