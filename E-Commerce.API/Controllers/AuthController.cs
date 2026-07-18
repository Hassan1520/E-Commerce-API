using System.Security.Claims;
using ECommerce.API.Extensions;
using ECommerce.API.Helpers;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Commands.ConfirmEmail;
using ECommerce.Application.Features.Auth.Commands.ForgetPassword;
using ECommerce.Application.Features.Auth.Commands.Login;
using ECommerce.Application.Features.Auth.Commands.RefreshToken;
using ECommerce.Application.Features.Auth.Commands.Register;
using ECommerce.Application.Features.Auth.Commands.ResetPassword;
using ECommerce.Application.Features.Auth.Commands.UpdateAvatar;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth-policy")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await mediator.Send(new RegisterCommand(request));
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registration successful. Please check your email to verify your account."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var result = await mediator.Send(new LoginCommand(request));
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }

    [HttpPost("avatar")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> UploadAvatar(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));

        await using var stream = file.OpenReadStream();
        var avatarUrl = await mediator.Send(new UpdateAvatarCommand(User.GetUserId(), stream, file.FileName, file.ContentType));
        return Ok(ApiResponse<object>.Ok(new { avatarUrl }, "Avatar updated successfully."));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request));
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token refreshed successfully."));
    }

    [HttpGet("verify-email")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail([FromQuery] int userId, [FromQuery] string token)
    {
        await mediator.Send(new ConfirmEmailCommand(userId, token));
        return Ok(ApiResponse<object>.Ok(null, "Email verified successfully! You can now log in."));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        await mediator.Send(new ForgotPasswordCommand(request));
        return Ok(ApiResponse<object>.Ok(null, "If the email exists, a password reset link has been sent."));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        await mediator.Send(new ResetPasswordCommand(request));
        return Ok(ApiResponse<object>.Ok(null, "Password has been reset successfully."));
    }
}
