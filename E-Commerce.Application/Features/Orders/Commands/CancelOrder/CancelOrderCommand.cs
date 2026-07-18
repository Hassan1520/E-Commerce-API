using ECommerce.Application.DTOs.Orders;
using MediatR;

namespace ECommerce.Application.Features.Orders.Commands.CancelOrder;

public record CancelOrderCommand(int UserId, int OrderId, bool IsAdmin) : IRequest<OrderDto>;
