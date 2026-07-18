using ECommerce.Application.Common;
using ECommerce.Application.DTOs.Products;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries.GetProductPage;

public record GetProductsPagedQuery(ProductSpecParams Params) : IRequest<PaginationResult<ProductDto>>;