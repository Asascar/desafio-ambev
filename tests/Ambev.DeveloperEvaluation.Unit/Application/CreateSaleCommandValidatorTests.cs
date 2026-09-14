using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for CreateSaleCommandValidator.
/// </summary>
public class CreateSaleCommandValidatorTests
{
    private readonly CreateSaleCommandValidator _validator = new();

    [Fact(DisplayName = "Given valid create sale command When validating Then validation passes")]
    public void Validate_ValidCommand_Passes()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand(2);

        // When
        var result = _validator.Validate(command);

        // Then
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Given command with empty items When validating Then validation fails")]
    public void Validate_EmptyItems_Fails()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand(0);

        // When
        var result = _validator.Validate(command);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Fact(DisplayName = "Given item with quantity greater than 20 When validating Then validation fails")]
    public void Validate_ItemQuantityAboveTwenty_Fails()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand(1);
        command.Items[0].Quantity = 25;

        // When
        var result = _validator.Validate(command);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Cannot sell more than 20"));
    }

    [Fact(DisplayName = "Given item with unit price zero or negative When validating Then validation fails")]
    public void Validate_ItemUnitPriceZero_Fails()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand(1);
        command.Items[0].UnitPrice = 0m;

        // When
        var result = _validator.Validate(command);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Unit price must be greater than zero"));
    }
}
