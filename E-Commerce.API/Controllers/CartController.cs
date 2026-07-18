using System.Security.Claims;
using ECommerce.API.Extensions;
using ECommerce.API.Helpers;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Features.Cart.Commands.CreateCart;
using ECommerce.Application.Features.Cart.Commands.DeleteCart;
using ECommerce.Application.Features.Cart.Commands.UpdateCart;
using ECommerce.Application.Features.Cart.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
    {
        var cart = await mediator.Send(new GetCartQuery(User.GetUserId()));
        return Ok(ApiResponse<CartDto>.Ok(cart));
    }

    [HttpPost("add")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddItem([FromBody] AddToCartDto dto)
    {
        var cart = await mediator.Send(new AddToCartCommand(User.GetUserId(), dto));
        return Ok(ApiResponse<CartDto>.Ok(cart, "Item added to cart."));
    }

    [HttpPost("remove")]
    public async Task<ActionResult<ApiResponse<CartDto>>> RemoveItem([FromBody] RemoveFromCartDto dto)
    {
        var cart = await mediator.Send(new RemoveFromCartCommand(User.GetUserId(), dto));
        return Ok(ApiResponse<CartDto>.Ok(cart, "Item removed from cart."));
    }

    [HttpPut("update")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateQuantity([FromBody] UpdateCartItemDto dto)
    {
        var cart = await mediator.Send(new UpdateCartItemCommand(User.GetUserId(), dto));
        return Ok(ApiResponse<CartDto>.Ok(cart, "Cart updated."));
    }

}
