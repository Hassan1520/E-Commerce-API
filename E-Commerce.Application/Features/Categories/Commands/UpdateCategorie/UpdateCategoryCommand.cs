using ECommerce.Application.DTOs.Categories;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands.UpdateCategorie;

public record UpdateCategoryCommand(int Id, UpdateCategoryDto Dto) : IRequest<CategoryDto>;