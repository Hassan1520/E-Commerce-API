using ECommerce.Application.DTOs.Categories;
using MediatR;

namespace ECommerce.Application.Features.Categories.Queries.GetCategorieById;

public record GetCategoryByIdQuery(int Id) : IRequest<CategoryDto>;