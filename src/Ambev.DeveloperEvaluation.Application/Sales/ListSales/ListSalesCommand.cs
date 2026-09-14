using Ambev.DeveloperEvaluation.Domain.Enums;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Command/query for listing sales with pagination, ordering, and filtering.
/// </summary>
public class ListSalesCommand : IRequest<ListSalesResult>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Order { get; set; }
    public string? Customer { get; set; }
    public string? Branch { get; set; }
    public SaleStatus? Status { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}
