using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// AutoMapper profile for CreateSale mappings.
/// </summary>
public class CreateSaleProfile : Profile
{
    public CreateSaleProfile()
    {
        CreateMap<Sale, CreateSaleResult>()
            .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => src.IsCancelled));

        CreateMap<SaleItem, CreateSaleItemResult>();
    }
}
