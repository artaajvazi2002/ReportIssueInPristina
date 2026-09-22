using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using ReportIssueInPristina.Application.Services;

namespace ReportIssueInPristina.Infrastructure.Services;

public sealed class AzureBlobImageStorageService : IImageStorageService
{
    private readonly BlobContainerClient _container;

    public AzureBlobImageStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"]
            ?? configuration.GetConnectionString("AzureBlobStorage")
            ?? throw new InvalidOperationException("Connection string 'AzureBlobStorage' is not configured.");
        var containerName = configuration["AzureBlobStorage:ContainerName"]
            ?? throw new InvalidOperationException("AzureBlobStorage:ContainerName is not configured.");

        _container = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blob = _container.GetBlobClient(fileName);
        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);

        if (!blob.CanGenerateSasUri)
        {
            throw new InvalidOperationException(
                "The Azure Blob connection string must contain an account key to generate image URLs.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = blob.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddDays(365)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blob.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(imageUrl);
        var path = uri.AbsolutePath.Trim('/');
        var containerPrefix = _container.Name + "/";
        var blobName = path.StartsWith(containerPrefix, StringComparison.OrdinalIgnoreCase)
            ? path[containerPrefix.Length..]
            : path;

        await _container.GetBlobClient(Uri.UnescapeDataString(blobName))
            .DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
