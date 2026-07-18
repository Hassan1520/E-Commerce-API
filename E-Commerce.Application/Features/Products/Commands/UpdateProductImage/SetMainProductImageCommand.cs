using MediatR;

namespace ECommerce.Application.Features.Products.Commands.UpdateProductImage;

public record SetMainProductImageCommand(int ProductId, int ImageId) : IRequest<Unit>;