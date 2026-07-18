using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using ECommerce.Infrastructure.Settings;

namespace ECommerce.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(IOptions<AzureStorageSettings> settings, ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = new BlobServiceClient(settings.Value.ConnectionString);
        _logger = logger;
    }

    public async Task<BlobUploadResult> UploadAsync(
        Stream fileStream, string fileName, string contentType, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var extension = Path.GetExtension(fileName);
        var blobName = $"{Guid.NewGuid()}{extension}";

        var blobClient = containerClient.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = headers });

        _logger.LogInformation("Uploaded blob {BlobName} to container {Container}", blobName, containerName);

        return new BlobUploadResult
        {
            Url = blobClient.Uri.ToString(),
            BlobName = blobName
        };
    }

    public async Task DeleteAsync(string blobName, string containerName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            return;

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var deleted = await blobClient.DeleteIfExistsAsync();

        if (deleted.Value)
            _logger.LogInformation("Deleted blob {BlobName} from container {Container}", blobName, containerName);
        else
            _logger.LogWarning("Blob {BlobName} not found in container {Container} (already deleted?)", blobName, containerName);
    }
}