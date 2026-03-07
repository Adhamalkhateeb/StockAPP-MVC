using Core.DTOs;
using FluentValidation;

namespace Core.Validators;

public class OrderRequestValidator<TOrderRequest> : AbstractValidator<TOrderRequest>
    where TOrderRequest : OrderRequest
{
    public OrderRequestValidator()
    {
        RuleFor(o => o.StockSymbol).NotEmpty().WithMessage("Stock Symbol can't be null or empty");

        RuleFor(o => o.StockName).NotEmpty().WithMessage("Stock Name can't be null or empty");

        RuleFor(o => o.DateAndTimeOfOrder)
            .Must(date => date == default || date >= new DateTime(2000, 01, 01))
            .WithMessage("Date of the order should not be older than Jan 01, 2000.");

        RuleFor(o => o.Quantity)
            .InclusiveBetween(1u, 100000u)
            .WithMessage("You can buy maximum of 100000 shares in single order. Minimum is 1.");

        RuleFor(o => o.Price)
            .InclusiveBetween(1u, 10000u)
            .WithMessage("The maximum Price stock 100000. Minimum is 1.");
    }
}

public class OrderRequestValidator : OrderRequestValidator<OrderRequest> { }
