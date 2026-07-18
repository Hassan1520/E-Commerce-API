using ECommerce.Application.DTOs.Categories;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands.CreateCategorie;

public record CreateCategoryCommand(CreateCategoryDto Dto) : IRequest<CategoryDto>;