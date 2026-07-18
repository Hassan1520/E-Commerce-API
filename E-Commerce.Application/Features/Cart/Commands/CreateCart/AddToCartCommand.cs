using ECommerce.Application.DTOs.Cart;
using MediatR;

namespace ECommerce.Application.Features.Cart.Commands.CreateCart;

public record AddToCartCommand(int UserId, AddToCartDto Dto) : IRequest<CartDto>;