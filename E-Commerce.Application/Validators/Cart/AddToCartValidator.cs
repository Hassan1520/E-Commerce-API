using ECommerce.Application.Features.Cart.Commands.CreateCart;
using FluentValidation;

namespace ECommerce.Application.Validators.Cart;

public class AddToCartValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.Dto.ProductId)
            .GreaterThan(0).WithMessage("Valid product is required.");

        RuleFor(x => x.Dto.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}
