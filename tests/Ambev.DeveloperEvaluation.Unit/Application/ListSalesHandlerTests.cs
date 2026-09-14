using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Unit tests for ListSalesHandler.
/// </summary>
public class ListSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new ListSalesHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid pagination query When listing sales Then returns paginated result")]
    public async Task Handle_ValidCommand_ReturnsPaginatedResult()
    {
        // Given
        var sales = new List<Sale> { SaleTestData.GenerateValidSale(1), SaleTestData.GenerateValidSale(2) };
        _saleRepository.GetPaginatedAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<SaleStatus?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Returns((sales, 2));

        _mapper.Map<List<SaleSummaryResult>>(sales).Returns(new List<SaleSummaryResult>
        {
            new() { Id = sales[0].Id, SaleNumber = sales[0].SaleNumber },
            new() { Id = sales[1].Id, SaleNumber = sales[1].SaleNumber }
        });

        var command = new ListSalesCommand { PageNumber = 1, PageSize = 10 };

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Sales.Should().HaveCount(2);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }
}
