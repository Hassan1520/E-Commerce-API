using ECommerce.Application.DTOs.Cart;
using MediatR;

namespace ECommerce.Application.Features.Cart.Commands.UpdateCart;

public record UpdateCartItemCommand(int UserId, UpdateCartItemDto Dto) : IRequest<CartDto>;