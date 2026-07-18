namespace ECommerce.Application.Interfaces.Services;

public interface ITokenService
{
    Task<(string AccessToken, DateTime ExpiresAt)> GenerateAccessTokenAsync(int userId, string name, string email, IEnumerable<string> roles);
    Task<string> GenerateRefreshTokenAsync(int userId);
    Task<(int UserId, string Name, string Email)?> ValidateRefreshTokenAsync(string refreshToken);
}
