using ECommerce.Application.DTOs.Products;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(CreateProductDto Dto) : IRequest<ProductDto>;