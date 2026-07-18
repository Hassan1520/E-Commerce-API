using ECommerce.Application.Features.Products.Commands.UpdateProduct;
using FluentValidation;

namespace ECommerce.Application.Validators.Products;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Dto.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Dto.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Dto.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");

        RuleFor(x => x.Dto.CategoryId)
            .GreaterThan(0).WithMessage("Valid category is required.");
    }
}
