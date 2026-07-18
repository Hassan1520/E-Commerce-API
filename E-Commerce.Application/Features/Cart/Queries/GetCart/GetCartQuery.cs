using ECommerce.Application.DTOs.Cart;
using MediatR;

namespace ECommerce.Application.Features.Cart.Queries.GetCart;

public record GetCartQuery(int UserId) : IRequest<CartDto>;