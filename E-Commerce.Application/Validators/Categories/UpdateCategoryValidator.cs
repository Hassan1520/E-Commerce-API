using ECommerce.Application.Features.Categories.Commands.UpdateCategorie;
using FluentValidation;

namespace ECommerce.Application.Validators.Categories;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100);
    }
}
