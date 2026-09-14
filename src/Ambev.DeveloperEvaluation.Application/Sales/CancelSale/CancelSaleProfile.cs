using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// AutoMapper profile for CancelSale mappings.
/// </summary>
public class CancelSaleProfile : Profile
{
    public CancelSaleProfile()
    {
        CreateMap<Sale, CancelSaleResult>()
            .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => src.IsCancelled));
    }
}
