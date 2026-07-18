using ECommerce.Application.DTOs.Orders;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(int UserId, int OrderId, bool IsAdmin) : IRequest<OrderDto>;