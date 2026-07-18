using ECommerce.Application.DTOs.Orders;
using MediatR;

namespace ECommerce.Application.Features.Orders.Commands.UpdateOrder;


public record UpdateOrderStatusCommand(int OrderId, UpdateOrderStatusDto Dto) : IRequest<OrderDto>;