using Ambev.DeveloperEvaluation.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

/// <summary>
/// Query parameters for listing sales.
/// Supports _page, _size, _order, and filters as defined in API documentation.
/// </summary>
public class ListSalesRequest
{
    [FromQuery(Name = "_page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "_size")]
    public int Size { get; set; } = 10;

    [FromQuery(Name = "_order")]
    public string? Order { get; set; }

    [FromQuery(Name = "customer")]
    public string? Customer { get; set; }

    [FromQuery(Name = "branch")]
    public string? Branch { get; set; }

    [FromQuery(Name = "status")]
    public SaleStatus? Status { get; set; }

    [FromQuery(Name = "_minDate")]
    public DateTime? MinDate { get; set; }

    [FromQuery(Name = "_maxDate")]
    public DateTime? MaxDate { get; set; }
}

/// <summary>
/// Response item summarizing a sale in the list view.
/// </summary>
public class SaleSummaryResponse
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public SaleStatus Status { get; set; }
    public bool IsCancelled { get; set; }
    public int TotalItems { get; set; }
    public DateTime CreatedAt { get; set; }
}
