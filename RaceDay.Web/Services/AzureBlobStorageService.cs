using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Web.Services;

public class AzureBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString =
            configuration["AzureBlobStorage:ConnectionString"];

        var containerName =
            configuration["AzureBlobStorage:ContainerName"];

        if (string.IsNullOrWhiteSpace(connectionString) ||
            string.IsNullOrWhiteSpace(containerName))
        {
            throw new InvalidOperationException(
                "Azure Blob Storage configuration is missing.");
        }

        var blobServiceClient =
            new BlobServiceClient(connectionString);

        _containerClient =
            blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        await _containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None);

        string extension =
            Path.GetExtension(Path.GetFileName(file.FileName));

        string blobName =
            $"{Guid.NewGuid()}{extension}";

        var blobClient =
            _containerClient.GetBlobClient(blobName);

        using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            });

        return blobName;
    }

    public async Task<(Stream Content, string ContentType)?> DownloadAsync(
        string blobName)
    {
        var blobClient =
            _containerClient.GetBlobClient(blobName);

        if (!(await blobClient.ExistsAsync()).Value)
        {
            return null;
        }

        var download =
            await blobClient.DownloadStreamingAsync();

        return (
            download.Value.Content,
            download.Value.Details.ContentType ?? "application/octet-stream"
        );
    }
}
