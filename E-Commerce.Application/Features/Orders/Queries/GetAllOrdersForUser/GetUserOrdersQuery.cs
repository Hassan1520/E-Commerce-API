using ECommerce.Application.DTOs.Orders;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.GetAllOrdersForUser;

public record GetUserOrdersQuery(int UserId) : IRequest<IEnumerable<OrderDto>>;