using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>
/// Unit tests for SaleItemValidator.
/// </summary>
public class SaleItemValidatorTests
{
    private readonly SaleItemValidator _validator = new();

    [Fact(DisplayName = "Given valid sale item When validating Then validation passes")]
    public void Validate_ValidItem_PassesValidation()
    {
        // Given
        var item = new SaleItem(Guid.NewGuid(), "Valid Product", 5, 25m);

        // When
        var result = _validator.Validate(item);

        // Then
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given empty product name When validating Then validation fails")]
    public void Validate_EmptyProductName_FailsValidation()
    {
        // Given
        var item = new SaleItem(Guid.NewGuid(), "Temp", 2, 10m)
        {
            ProductName = string.Empty
        };

        // When
        var result = _validator.Validate(item);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductName");
    }
}
