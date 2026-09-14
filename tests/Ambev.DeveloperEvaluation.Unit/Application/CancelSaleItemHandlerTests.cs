using FluentAssertions;
using NSubstitute;
using Xunit;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for CancelSaleItemHandler.
/// </summary>
public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _handler = new CancelSaleItemHandler(_saleRepository, _eventPublisher);
    }

    [Fact(DisplayName = "Given existing sale and item When cancelling item Then cancels item and recalculates total")]
    public async Task Handle_ExistingItem_CancelsItemAndUpdatesTotal()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var targetItem = sale.Items.First();

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>())
            .Returns(sale);

        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);

        var command = new CancelSaleItemCommand(sale.Id, targetItem.Id);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.ItemId.Should().Be(targetItem.Id);
        result.IsCancelled.Should().BeTrue();
        targetItem.IsCancelled.Should().BeTrue();
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await _eventPublisher.Received().PublishAsync(Arg.Any<ItemCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale When cancelling item Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentSale_ThrowsKeyNotFoundException()
    {
        // Given
        var nonExistentSaleId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(nonExistentSaleId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var command = new CancelSaleItemCommand(nonExistentSaleId, Guid.NewGuid());

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
