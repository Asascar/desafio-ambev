using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Command/query for retrieving a sale by ID.
/// </summary>
public class GetSaleCommand : IRequest<GetSaleResult>
{
    public Guid Id { get; }

    public GetSaleCommand(Guid id)
    {
        Id = id;
    }
}
