using ECommerce.Application.DTOs.Categories;
using MediatR;

namespace ECommerce.Application.Features.Categories.Queries.GetAllCategories
{
    public record GetAllCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;

}
