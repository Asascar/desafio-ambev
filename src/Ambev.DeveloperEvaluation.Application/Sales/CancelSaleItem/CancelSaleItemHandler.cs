using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Handler for processing CancelSaleItemCommand requests.
/// </summary>
public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IEventPublisher _eventPublisher;

    public CancelSaleItemHandler(
        ISaleRepository saleRepository,
        IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID '{command.SaleId}' not found.");

        sale.CancelItem(command.ItemId);

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        // Publish domain events
        foreach (var domainEvent in updatedSale.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }
        updatedSale.ClearDomainEvents();

        return new CancelSaleItemResult
        {
            SaleId = updatedSale.Id,
            ItemId = command.ItemId,
            NewSaleTotalAmount = updatedSale.TotalAmount,
            IsCancelled = true
        };
    }
}
