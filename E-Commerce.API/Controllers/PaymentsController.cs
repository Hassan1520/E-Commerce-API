using System.Security.Claims;
using ECommerce.API.Extensions;
using ECommerce.API.Helpers;
using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Features.Payments.Commands.CreateCheckout;
using ECommerce.Application.Features.Payments.Commands.HandleStripeWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpPost("checkout")]
    [Authorize]
    [EnableRateLimiting("checkout-policy")]
    public async Task<ActionResult<ApiResponse<CheckoutResponseDto>>> Checkout()
    {
        var result = await mediator.Send(new CreateCheckoutCommand(User.GetUserId()));
        return Ok(ApiResponse<CheckoutResponseDto>.Ok(result, "Payment intent created successfully."));
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var json = await reader.ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
        await mediator.Send(new HandleStripeWebhookCommand(json, stripeSignature));
        return Ok();
    }

}
