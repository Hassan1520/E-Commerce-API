using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.DTOs.Auth;

public class UploadAvatarDto
{
    public IFormFile File { get; set; } = null!;
}