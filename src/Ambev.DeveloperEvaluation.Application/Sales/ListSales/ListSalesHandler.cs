using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Handler for processing ListSalesCommand requests.
/// </summary>
public class ListSalesHandler : IRequestHandler<ListSalesCommand, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesCommand command, CancellationToken cancellationToken)
    {
        var validator = new ListSalesValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var (sales, totalCount) = await _saleRepository.GetPaginatedAsync(
            command.PageNumber,
            command.PageSize,
            command.Order,
            command.Customer,
            command.Branch,
            command.Status,
            command.MinDate,
            command.MaxDate,
            cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)command.PageSize);

        return new ListSalesResult
        {
            Sales = _mapper.Map<List<SaleSummaryResult>>(sales),
            TotalCount = totalCount,
            CurrentPage = command.PageNumber,
            TotalPages = totalPages
        };
    }
}
