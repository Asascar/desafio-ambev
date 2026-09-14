using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Validator for ListSalesCommand.
/// </summary>
public class ListSalesValidator : AbstractValidator<ListSalesCommand>
{
    public ListSalesValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than zero.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

        When(x => x.MinDate.HasValue && x.MaxDate.HasValue, () =>
        {
            RuleFor(x => x.MinDate!.Value)
                .LessThanOrEqualTo(x => x.MaxDate!.Value)
                .WithMessage("MinDate cannot be later than MaxDate.");
        });
    }
}
