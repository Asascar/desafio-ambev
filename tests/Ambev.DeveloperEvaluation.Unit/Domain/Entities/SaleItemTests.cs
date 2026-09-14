using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="SaleItem"/> entity, focusing on business rules for quantity-based discounting.
/// </summary>
public class SaleItemTests
{
    [Theory(DisplayName = "Given quantity below 4 When creating item Then applies 0% discount")]
    [InlineData(1, 100, 0, 100)]
    [InlineData(2, 50, 0, 100)]
    [InlineData(3, 100, 0, 300)]
    public void Constructor_QuantityBelowFour_AppliesZeroDiscount(int quantity, decimal unitPrice, decimal expectedDiscount, decimal expectedTotal)
    {
        // When
        var item = new SaleItem(Guid.NewGuid(), "Beer Heineken 330ml", quantity, unitPrice);

        // Then
        item.DiscountPercentage.Should().Be(0.0m);
        item.Discount.Should().Be(expectedDiscount);
        item.TotalAmount.Should().Be(expectedTotal);
        item.IsCancelled.Should().BeFalse();
    }

    [Theory(DisplayName = "Given quantity between 4 and 9 When creating item Then applies 10% discount")]
    [InlineData(4, 100, 40, 360)]
    [InlineData(5, 20, 10, 90)]
    [InlineData(9, 100, 90, 810)]
    public void Constructor_QuantityBetweenFourAndNine_AppliesTenPercentDiscount(int quantity, decimal unitPrice, decimal expectedDiscount, decimal expectedTotal)
    {
        // When
        var item = new SaleItem(Guid.NewGuid(), "Beer Corona 355ml", quantity, unitPrice);

        // Then
        item.DiscountPercentage.Should().Be(0.10m);
        item.Discount.Should().Be(expectedDiscount);
        item.TotalAmount.Should().Be(expectedTotal);
    }

    [Theory(DisplayName = "Given quantity between 10 and 20 When creating item Then applies 20% discount")]
    [InlineData(10, 100, 200, 800)]
    [InlineData(15, 10, 30, 120)]
    [InlineData(20, 50, 200, 800)]
    public void Constructor_QuantityBetweenTenAndTwenty_AppliesTwentyPercentDiscount(int quantity, decimal unitPrice, decimal expectedDiscount, decimal expectedTotal)
    {
        // When
        var item = new SaleItem(Guid.NewGuid(), "Beer Stella Artois 330ml", quantity, unitPrice);

        // Then
        item.DiscountPercentage.Should().Be(0.20m);
        item.Discount.Should().Be(expectedDiscount);
        item.TotalAmount.Should().Be(expectedTotal);
    }

    [Theory(DisplayName = "Given quantity above 20 When creating item Then throws DomainException")]
    [InlineData(21)]
    [InlineData(25)]
    [InlineData(100)]
    public void Constructor_QuantityAboveTwenty_ThrowsDomainException(int quantity)
    {
        // When
        var act = () => new SaleItem(Guid.NewGuid(), "Beer Budweiser", quantity, 10m);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage($"*Cannot sell more than {SaleItem.MaxAllowedQuantity} identical items*");
    }

    [Theory(DisplayName = "Given quantity zero or negative When creating item Then throws DomainException")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_QuantityZeroOrNegative_ThrowsDomainException(int quantity)
    {
        // When
        var act = () => new SaleItem(Guid.NewGuid(), "Beer Brahma", quantity, 10m);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("*Quantity must be greater than zero*");
    }

    [Theory(DisplayName = "Given unit price zero or negative When creating item Then throws DomainException")]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_UnitPriceZeroOrNegative_ThrowsDomainException(decimal unitPrice)
    {
        // When
        var act = () => new SaleItem(Guid.NewGuid(), "Beer Skol", 5, unitPrice);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("*Unit price must be greater than zero*");
    }

    [Fact(DisplayName = "Given active item When cancelling Then marks item as cancelled")]
    public void Cancel_ActiveItem_MarksAsCancelled()
    {
        // Given
        var item = new SaleItem(Guid.NewGuid(), "Beer Antarctica", 4, 10m);

        // When
        item.Cancel();

        // Then
        item.IsCancelled.Should().BeTrue();
        item.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given active item When updating quantity and price Then recalculates discount and total")]
    public void Update_ActiveItem_RecalculatesDiscountAndTotal()
    {
        // Given
        var item = new SaleItem(Guid.NewGuid(), "Beer Colorado", 3, 20m); // 0% discount, total 60
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(60m);

        // When
        item.Update(10, 20m); // 20% discount on 200 = 40, total = 160

        // Then
        item.Quantity.Should().Be(10);
        item.DiscountPercentage.Should().Be(0.20m);
        item.Discount.Should().Be(40m);
        item.TotalAmount.Should().Be(160m);
        item.UpdatedAt.Should().NotBeNull();
    }
}
