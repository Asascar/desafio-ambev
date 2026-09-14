using Ambev.DeveloperEvaluation.Domain.Enums;
using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;

/// <summary>
/// Response returned after cancelling a sale.
/// </summary>
public class CancelSaleResponse
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public SaleStatus Status { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CancelSaleProfile : Profile
{
    public CancelSaleProfile()
    {
        CreateMap<CancelSaleResult, CancelSaleResponse>();
    }
}
