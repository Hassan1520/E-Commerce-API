using ECommerce.Application.Common;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Web;

namespace ECommerce.Infrastructure.Services;

public class AuthIdentityService : IAuthIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IBlobStorageService _blobStorageService;

    public AuthIdentityService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IEmailSenderService emailSenderService,
        IBlobStorageService blobStorageService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailSenderService = emailSenderService;
        _blobStorageService = blobStorageService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            throw new ConflictException("Email is already registered.");

        var user = new User
        {
            Name = request.Name,
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new AppException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, UserRole.Customer.ToString());

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        var confirmationLink = $"{request.ClientUrl}?userId={user.Id}&token={encodedToken}";

        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 5px;'>
                <h2 style='color: #333;'>Welcome to our E-Commerce Store, {user.Name}!</h2>
                <p>Please confirm your email address to activate your account by clicking the button below:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{confirmationLink}' style='background-color: #007bff; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Confirm Email Address</a>
                </div>
                <p style='color: #777; font-size: 12px;'>If the button doesn't work, copy and paste this link into your browser:<br>{confirmationLink}</p>
            </div>";

        await _emailSenderService.SendEmailAsync(user.Email!, "Confirm Your Email", emailBody);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email!,
            Roles = [UserRole.Customer.ToString()],
            Role = UserRole.Customer.ToString()
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            throw new UnauthorizedException("Invalid email or password.");

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (signInResult.IsNotAllowed)
            throw new ForbiddenException("You must confirm your email before logging in.");

        if (!signInResult.Succeeded)
            throw new UnauthorizedException("Invalid email or password.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task ConfirmEmailAsync(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        var finalToken = token.Contains('%') ? HttpUtility.UrlDecode(token) : token;
        var result = await _userManager.ConfirmEmailAsync(user, finalToken);

        if (!result.Succeeded)
            throw new NotFoundException("Invalid or expired email confirmation token.");
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(token);
        var resetLink = $"{request.ClientUrl}?email={user.Email}&token={encodedToken}";

        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <h3>Reset Your Password</h3>
                <p>We received a request to reset your password. Click the button below to proceed:</p>
                <div style='text-align: center; margin: 20px 0;'>
                    <a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a>
                </div>
                <p>If you didn't request this, you can safely ignore this email.</p>
            </div>";

        await _emailSenderService.SendEmailAsync(user.Email!, "Reset Password Request", emailBody);
    }

    public async Task<string> UpdateAvatarAsync(int userId, Stream fileStream, string fileName, string contentType)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        if (!string.IsNullOrEmpty(user.AvatarBlobName))
            await _blobStorageService.DeleteAsync(user.AvatarBlobName, AzureStorageContainers.UserAvatars);

        var uploadResult = await _blobStorageService.UploadAsync(
            fileStream, fileName, contentType, AzureStorageContainers.UserAvatars);

        user.AvatarUrl = uploadResult.Url;
        user.AvatarBlobName = uploadResult.BlobName;
        await _userManager.UpdateAsync(user);
        return uploadResult.Url;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new NotFoundException("User not found.");

        var finalToken = request.Token.Contains('%') ? HttpUtility.UrlDecode(request.Token) : request.Token;
        var result = await _userManager.ResetPasswordAsync(user, finalToken, request.NewPassword);

        if (!result.Succeeded)
            throw new AppException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var validated = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (validated is null)
            throw new UnauthorizedException("Invalid refresh token.");

        var user = await _userManager.FindByIdAsync(validated.Value.UserId.ToString())
            ?? throw new UnauthorizedException("Invalid refresh token.");

        return await BuildAuthResponseAsync(user);
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(
            user.Id, user.Name, user.Email ?? string.Empty, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList(),
            Role = roles.FirstOrDefault() ?? string.Empty,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = expiresAt
        };
    }
}
