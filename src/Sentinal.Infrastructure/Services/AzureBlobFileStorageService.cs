using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Options;

namespace Sentinal.Infrastructure.Services;

public class AzureBlobFileStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobFileStorageService(IOptions<AzureBlobStorageOptions> options)
    {
        var blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
    }

    public async Task<bool> SaveFileAsync(Guid userId, Guid fileId, Stream fileContent)
    {
        var blobName = $"{userId}/{fileId}";
        await _containerClient.GetBlobClient(blobName).UploadAsync(fileContent, overwrite: true);
        return true;
    }

    public async Task<Stream> GetFileAsync(Guid userId, Guid fileId)
    {
        var blobName = $"{userId}/{fileId}";
        var download = await _containerClient.GetBlobClient(blobName).DownloadAsync();
        return download.Value.Content;
    }

    public async Task<bool> CreateRootFolderAsync(Guid userId)
    {
        await _containerClient.CreateIfNotExistsAsync();
        return true;
    }

    public async Task<bool> DeleteFileAsync(Guid userId, Guid fileId)
    {
        var blobName = $"{userId}/{fileId}";
        await _containerClient.GetBlobClient(blobName).DeleteAsync();
        return true;
    }

    public async Task<bool> FileExistsAsync(Guid userId, Guid fileId)
    {
        var blobName = $"{userId}/{fileId}";
        return await _containerClient.GetBlobClient(blobName).ExistsAsync();
    }
}