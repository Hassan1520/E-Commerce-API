using ECommerce.Application.DTOs.Cart;
using MediatR;

namespace ECommerce.Application.Features.Cart.Commands.DeleteCart;

public record RemoveFromCartCommand(int UserId, RemoveFromCartDto Dto) : IRequest<CartDto>;