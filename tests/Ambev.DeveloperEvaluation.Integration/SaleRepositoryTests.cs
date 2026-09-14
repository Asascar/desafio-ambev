using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

/// <summary>
/// Integration tests for SaleRepository using EF Core In-Memory database.
/// </summary>
public class SaleRepositoryTests : IDisposable
{
    private readonly DefaultContext _context;
    private readonly SaleRepository _repository;

    public SaleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: $"Integration_Sales_{Guid.NewGuid()}")
            .Options;

        _context = new DefaultContext(options);
        _context.Database.EnsureCreated();
        _repository = new SaleRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact(DisplayName = "Given valid sale When creating in repository Then persists sale and items")]
    public async Task CreateAsync_ValidSale_PersistsSuccessfully()
    {
        // Given
        var sale = new Sale("SALE-INT-001", DateTime.UtcNow, Guid.NewGuid(), "Customer InMem", Guid.NewGuid(), "Branch InMem");
        sale.AddItem(Guid.NewGuid(), "Product A", 5, 20m);
        sale.AddItem(Guid.NewGuid(), "Product B", 2, 50m);

        // When
        var created = await _repository.CreateAsync(sale);

        // Then
        created.Should().NotBeNull();
        created.Id.Should().NotBeEmpty();

        var retrieved = await _repository.GetByIdAsync(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.SaleNumber.Should().Be("SALE-INT-001");
        retrieved.Items.Should().HaveCount(2);
        retrieved.TotalAmount.Should().Be(190m);
    }

    [Fact(DisplayName = "Given existing sale When querying by sale number Then returns sale with items")]
    public async Task GetBySaleNumberAsync_ExistingSale_ReturnsSale()
    {
        // Given
        var sale = new Sale("SALE-INT-NUM", DateTime.UtcNow, Guid.NewGuid(), "Customer Alpha", Guid.NewGuid(), "Branch Alpha");
        sale.AddItem(Guid.NewGuid(), "Product Alpha", 10, 15m); // 150 - 20% = 120
        await _repository.CreateAsync(sale);

        // When
        var retrieved = await _repository.GetBySaleNumberAsync("SALE-INT-NUM");

        // Then
        retrieved.Should().NotBeNull();
        retrieved!.SaleNumber.Should().Be("SALE-INT-NUM");
        retrieved.TotalAmount.Should().Be(120m);
    }

    [Fact(DisplayName = "Given multiple sales When querying paginated with filters Then returns matching sales")]
    public async Task GetPaginatedAsync_WithFilters_ReturnsFilteredSales()
    {
        // Given
        var branchAlphaId = Guid.NewGuid();
        var branchBetaId = Guid.NewGuid();

        var sale1 = new Sale("SALE-PAG-01", DateTime.UtcNow.AddDays(-2), Guid.NewGuid(), "Ambev User 1", branchAlphaId, "Branch Alpha");
        sale1.AddItem(Guid.NewGuid(), "Product 1", 2, 10m);
        await _repository.CreateAsync(sale1);

        var sale2 = new Sale("SALE-PAG-02", DateTime.UtcNow.AddDays(-1), Guid.NewGuid(), "Ambev User 2", branchBetaId, "Branch Beta");
        sale2.AddItem(Guid.NewGuid(), "Product 2", 4, 10m);
        await _repository.CreateAsync(sale2);

        // When: filter by customer 'User 1'
        var (sales, totalCount) = await _repository.GetPaginatedAsync(
            pageNumber: 1,
            pageSize: 10,
            customer: "User 1");

        // Then
        totalCount.Should().Be(1);
        sales.Should().ContainSingle(s => s.SaleNumber == "SALE-PAG-01");
    }

    [Fact(DisplayName = "Given existing sale When deleting Then removes from database")]
    public async Task DeleteAsync_ExistingSale_RemovesSuccessfully()
    {
        // Given
        var sale = new Sale("SALE-INT-DEL", DateTime.UtcNow, Guid.NewGuid(), "Customer Del", Guid.NewGuid(), "Branch Del");
        sale.AddItem(Guid.NewGuid(), "Product Del", 1, 50m);
        await _repository.CreateAsync(sale);

        // When
        var success = await _repository.DeleteAsync(sale.Id);

        // Then
        success.Should().BeTrue();
        var retrieved = await _repository.GetByIdAsync(sale.Id);
        retrieved.Should().BeNull();
    }
}
