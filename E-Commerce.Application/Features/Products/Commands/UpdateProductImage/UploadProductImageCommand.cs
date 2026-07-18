using ECommerce.Application.DTOs.Products;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands.UpdateProductImage;

public record UploadProductImageCommand(int ProductId, Stream FileStream, string FileName, string ContentType) : IRequest<ProductImageDto>;