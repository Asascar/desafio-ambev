using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>
/// Unit tests for SaleValidator.
/// </summary>
public class SaleValidatorTests
{
    private readonly SaleValidator _validator = new();

    [Fact(DisplayName = "Given valid sale with items When validating Then validation passes")]
    public void Validate_ValidSale_PassesValidation()
    {
        // Given
        var sale = new Sale("SALE-123", DateTime.UtcNow, Guid.NewGuid(), "Customer Name", Guid.NewGuid(), "Branch Name");
        sale.AddItem(Guid.NewGuid(), "Product A", 5, 20m);

        // When
        var result = _validator.Validate(sale);

        // Then
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given sale without items When validating Then validation fails")]
    public void Validate_SaleWithoutItems_FailsValidation()
    {
        // Given
        var sale = new Sale("SALE-124", DateTime.UtcNow, Guid.NewGuid(), "Customer Name", Guid.NewGuid(), "Branch Name");

        // When
        var result = _validator.Validate(sale);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Fact(DisplayName = "Given sale with empty sale number When validating Then validation fails")]
    public void Validate_EmptySaleNumber_FailsValidation()
    {
        // Given
        var sale = new Sale("SALE-125", DateTime.UtcNow, Guid.NewGuid(), "Customer Name", Guid.NewGuid(), "Branch Name")
        {
            SaleNumber = string.Empty
        };
        sale.AddItem(Guid.NewGuid(), "Product B", 2, 10m);

        // When
        var result = _validator.Validate(sale);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SaleNumber");
    }
}
