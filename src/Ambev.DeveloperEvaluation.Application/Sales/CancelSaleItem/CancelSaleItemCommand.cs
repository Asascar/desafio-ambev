using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Command for cancelling a specific item in a sale.
/// </summary>
public class CancelSaleItemCommand : IRequest<CancelSaleItemResult>
{
    public Guid SaleId { get; }
    public Guid ItemId { get; }

    public CancelSaleItemCommand(Guid saleId, Guid itemId)
    {
        SaleId = saleId;
        ItemId = itemId;
    }
}
