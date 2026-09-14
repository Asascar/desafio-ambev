using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating test data for Sales using the Bogus library.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Generates a valid Sale entity with randomized data and active items.
    /// </summary>
    public static Sale GenerateValidSale(int itemCount = 2)
    {
        var sale = new Sale(
            saleNumber: $"SALE-{Faker.Random.Number(10000, 99999)}",
            saleDate: DateTime.UtcNow,
            customerId: Guid.NewGuid(),
            customerName: Faker.Name.FullName(),
            branchId: Guid.NewGuid(),
            branchName: Faker.Company.CompanyName());

        for (int i = 0; i < itemCount; i++)
        {
            var quantity = Faker.Random.Number(1, 20);
            var unitPrice = Math.Round(Faker.Random.Decimal(10m, 200m), 2);
            sale.AddItem(Guid.NewGuid(), Faker.Commerce.ProductName(), quantity, unitPrice);
        }

        return sale;
    }

    /// <summary>
    /// Generates a valid SaleItem with specified or randomized parameters.
    /// </summary>
    public static SaleItem GenerateValidSaleItem(int? quantity = null, decimal? unitPrice = null)
    {
        var q = quantity ?? Faker.Random.Number(1, 20);
        var p = unitPrice ?? Math.Round(Faker.Random.Decimal(10m, 100m), 2);
        return new SaleItem(Guid.NewGuid(), Faker.Commerce.ProductName(), q, p);
    }

    /// <summary>
    /// Generates a valid CreateSaleCommand for testing application handlers.
    /// </summary>
    public static CreateSaleCommand GenerateValidCreateSaleCommand(int itemCount = 2)
    {
        var command = new CreateSaleCommand
        {
            SaleNumber = $"SALE-{Faker.Random.Number(10000, 99999)}",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Name.FullName(),
            BranchId = Guid.NewGuid(),
            BranchName = Faker.Company.CompanyName(),
            Items = new List<CreateSaleItemCommand>()
        };

        for (int i = 0; i < itemCount; i++)
        {
            command.Items.Add(new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = Faker.Commerce.ProductName(),
                Quantity = Faker.Random.Number(1, 15),
                UnitPrice = Math.Round(Faker.Random.Decimal(10m, 150m), 2)
            });
        }

        return command;
    }

    /// <summary>
    /// Generates a valid UpdateSaleCommand.
    /// </summary>
    public static UpdateSaleCommand GenerateValidUpdateSaleCommand(Guid saleId, int itemCount = 2)
    {
        var command = new UpdateSaleCommand
        {
            Id = saleId,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Name.FullName(),
            BranchId = Guid.NewGuid(),
            BranchName = Faker.Company.CompanyName(),
            Items = new List<UpdateSaleItemCommand>()
        };

        for (int i = 0; i < itemCount; i++)
        {
            command.Items.Add(new UpdateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = Faker.Commerce.ProductName(),
                Quantity = Faker.Random.Number(1, 10),
                UnitPrice = Math.Round(Faker.Random.Decimal(10m, 100m), 2)
            });
        }

        return command;
    }
}
