using System.Security.Claims;
using ECommerce.API.Extensions;
using ECommerce.API.Helpers;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Features.Orders.Commands.CancelOrder;
using ECommerce.Application.Features.Orders.Commands.UpdateOrder;
using ECommerce.Application.Features.Orders.Queries.GetAllOrdersForAdmin;
using ECommerce.Application.Features.Orders.Queries.GetAllOrdersForUser;
using ECommerce.Application.Features.Orders.Queries.GetOrderById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("checkout-policy")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet("my-orders")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetMyOrders()
    {
        var orders = await mediator.Send(new GetUserOrdersQuery(User.GetUserId()));
        return Ok(ApiResponse<IEnumerable<OrderDto>>.Ok(orders));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(int id)
    {
        var order = await mediator.Send(new GetOrderByIdQuery(User.GetUserId(), id, IsAdmin()));
        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    [HttpGet("admin-all-orders")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetAllOrdersForAdmin()
    {
        var orders = await mediator.Send(new GetAllOrdersForAdminQuery());
        return Ok(ApiResponse<IEnumerable<OrderDto>>.Ok(orders));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await mediator.Send(new UpdateOrderStatusCommand(id, dto));
        return Ok(ApiResponse<OrderDto>.Ok(order, $"Order status updated to '{dto.Status}' successfully."));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CancelOrder(int id)
    {
        var order = await mediator.Send(new CancelOrderCommand(User.GetUserId(), id, IsAdmin()));
        return Ok(ApiResponse<OrderDto>.Ok(order, "Order cancelled successfully."));
    }

    private bool IsAdmin() => User.IsInRole("Admin");
}
