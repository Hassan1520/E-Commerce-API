namespace ECommerce.Application.Interfaces.Services;

public class BlobUploadResult
{
    public string Url { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty;
}

public interface IBlobStorageService
{
    Task<BlobUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType, string containerName);
    Task DeleteAsync(string blobName, string containerName);
}