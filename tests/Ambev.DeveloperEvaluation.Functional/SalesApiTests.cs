using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class SalesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SalesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/sales should create sale with correct discounts and return 201 Created")]
    public async Task CreateSale_ValidRequest_ReturnsCreated()
    {
        // Given
        var request = new CreateSaleRequest
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Functional Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Functional Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product 10pct",
                    Quantity = 5,
                    UnitPrice = 20m
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product 20pct",
                    Quantity = 10,
                    UnitPrice = 15m
                }
            }
        };

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);

        // Item 1: 5 * 20 = 100 - 10% = 90
        var item1 = result.Data.Items.First(i => i.Quantity == 5);
        item1.DiscountPercentage.Should().Be(0.10m);
        item1.Discount.Should().Be(10m);
        item1.TotalAmount.Should().Be(90m);

        // Item 2: 10 * 15 = 150 - 20% = 120
        var item2 = result.Data.Items.First(i => i.Quantity == 10);
        item2.DiscountPercentage.Should().Be(0.20m);
        item2.Discount.Should().Be(30m);
        item2.TotalAmount.Should().Be(120m);

        // Total sale amount: 90 + 120 = 210
        result.Data.TotalAmount.Should().Be(210m);
    }

    [Fact(DisplayName = "POST /api/sales with quantity > 20 should return 400 BadRequest")]
    public async Task CreateSale_QuantityGreaterThan20_ReturnsBadRequest()
    {
        // Given
        var request = new CreateSaleRequest
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Invalid Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product Exceeded",
                    Quantity = 21,
                    UnitPrice = 10m
                }
            }
        };

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "GET /api/sales/{id} should return 200 OK with sale details")]
    public async Task GetSale_ExistingId_ReturnsOk()
    {
        // Given: create sale first
        var createRequest = new CreateSaleRequest
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Get Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Get Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "P1", Quantity = 2, UnitPrice = 50m }
            }
        };
        var createRes = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var created = await createRes.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>();
        var saleId = created!.Data!.Id;

        // When
        var response = await _client.GetAsync($"/api/sales/{saleId}");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<GetSaleResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(saleId);
        result.Data.CustomerName.Should().Be("Get Test Customer");
    }

    [Fact(DisplayName = "GET /api/sales should return paginated list compliant with general-api.md")]
    public async Task ListSales_Paginated_ReturnsPagedResults()
    {
        // Given: create a sale
        var createRequest = new CreateSaleRequest
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "List Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "List Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "P1", Quantity = 1, UnitPrice = 10m }
            }
        };
        await _client.PostAsJsonAsync("/api/sales", createRequest);

        // When
        var response = await _client.GetAsync("/api/sales?_page=1&_size=10&_order=saleDate desc");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SaleSummaryResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        result.TotalItems.Should().BeGreaterThan(0);
        result.CurrentPage.Should().Be(1);
    }

    [Fact(DisplayName = "PATCH /api/sales/{id}/cancel should cancel sale and return 200 OK")]
    public async Task CancelSale_ExistingSale_ReturnsCancelled()
    {
        // Given: create sale
        var createRequest = new CreateSaleRequest
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Cancel Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Cancel Branch",
            Items = new List<CreateSaleItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "P1", Quantity = 3, UnitPrice = 10m }
            }
        };
        var createRes = await _client.PostAsJsonAsync("/api/sales", createRequest);
        var created = await createRes.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>();
        var saleId = created!.Data!.Id;

        // When
        var response = await _client.PatchAsync($"/api/sales/{saleId}/cancel", null);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponseWithData<CancelSaleResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.IsCancelled.Should().BeTrue();

        // Verify with GET
        var getRes = await _client.GetAsync($"/api/sales/{saleId}");
        var verified = await getRes.Content.ReadFromJsonAsync<ApiResponseWithData<GetSaleResponse>>();
        verified!.Data!.IsCancelled.Should().BeTrue();
    }
}
