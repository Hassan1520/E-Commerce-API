using MediatR;

namespace ECommerce.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest<Unit>;
