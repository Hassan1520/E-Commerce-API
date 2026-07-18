using ECommerce.Application.Features.Cart.Commands.UpdateCart;
using FluentValidation;

namespace ECommerce.Application.Validators.Cart;

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.Dto.ProductId)
            .GreaterThan(0).WithMessage("Valid product is required.");

        RuleFor(x => x.Dto.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}
