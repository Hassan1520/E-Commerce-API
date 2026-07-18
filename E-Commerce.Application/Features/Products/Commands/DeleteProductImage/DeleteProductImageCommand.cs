using MediatR;

namespace ECommerce.Application.Features.Products.Commands.DeleteProductImage;

public record DeleteProductImageCommand(int ProductId, int ImageId) : IRequest<Unit>;