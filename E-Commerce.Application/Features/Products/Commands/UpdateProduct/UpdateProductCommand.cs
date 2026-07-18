using ECommerce.Application.DTOs.Products;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(int Id, UpdateProductDto Dto) : IRequest<ProductDto>;