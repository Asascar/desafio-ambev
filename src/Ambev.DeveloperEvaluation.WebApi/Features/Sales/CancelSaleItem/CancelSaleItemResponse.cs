using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

/// <summary>
/// Response returned after cancelling an item in a sale.
/// </summary>
public class CancelSaleItemResponse
{
    public Guid SaleId { get; set; }
    public Guid ItemId { get; set; }
    public decimal NewSaleTotalAmount { get; set; }
    public bool IsCancelled { get; set; }
}

public class CancelSaleItemProfile : Profile
{
    public CancelSaleItemProfile()
    {
        CreateMap<CancelSaleItemResult, CancelSaleItemResponse>();
    }
}
