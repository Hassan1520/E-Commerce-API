using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.DTOs.Products;

public class UploadProductImageDto
{
    public IFormFile File { get; set; } = null!;
}