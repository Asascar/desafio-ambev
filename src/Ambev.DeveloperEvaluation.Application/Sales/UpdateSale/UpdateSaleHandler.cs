using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests.
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public UpdateSaleHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID '{command.Id}' not found.");

        sale.UpdateDetails(
            command.SaleDate == default ? sale.SaleDate : command.SaleDate,
            command.CustomerId,
            command.CustomerName,
            command.BranchId,
            command.BranchName);

        // Synchronize items:
        // 1. Cancel active items that are no longer in the updated command
        var commandProductIds = command.Items.Select(i => i.ProductId).ToHashSet();
        foreach (var item in sale.Items.Where(i => !i.IsCancelled))
        {
            if (!commandProductIds.Contains(item.ProductId))
            {
                sale.CancelItem(item.Id);
            }
        }

        // 2. Update existing active items or add new ones
        foreach (var commandItem in command.Items)
        {
            var existingItem = sale.Items.FirstOrDefault(i => i.ProductId == commandItem.ProductId && !i.IsCancelled);
            if (existingItem != null)
            {
                existingItem.Update(commandItem.Quantity, commandItem.UnitPrice);
            }
            else
            {
                sale.AddItem(commandItem.ProductId, commandItem.ProductName, commandItem.Quantity, commandItem.UnitPrice);
            }
        }

        sale.RecalculateTotals();

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        // Publish domain events
        foreach (var domainEvent in updatedSale.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }
        updatedSale.ClearDomainEvents();

        return _mapper.Map<UpdateSaleResult>(updatedSale);
    }
}
