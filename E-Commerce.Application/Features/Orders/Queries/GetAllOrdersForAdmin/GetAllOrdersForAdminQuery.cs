using ECommerce.Application.DTOs.Orders;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.GetAllOrdersForAdmin;

public record GetAllOrdersForAdminQuery : IRequest<IEnumerable<OrderDto>>;