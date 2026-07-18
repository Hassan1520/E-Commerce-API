using ECommerce.Application.Features.Categories.Commands.CreateCategorie;
using FluentValidation;

namespace ECommerce.Application.Validators.Categories;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100);
    }
}
