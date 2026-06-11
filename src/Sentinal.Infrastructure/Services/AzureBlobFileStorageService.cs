using Microsoft.Extensions.Options;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Options;

namespace Sentinal.Infrastructure.Services;

public class AzureBlobFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _config;
    public AzureBlobFileStorageService(IOptions<FileStorageOptions> config)
    {
        _config = config.Value;
    }

    public Task<bool> SaveFileAsync(Guid userId, Guid folderId, Guid fileId, Stream fileContent)
    {
        throw new NotImplementedException();
    }

    public Task<bool> FileExistsAsync(Guid userId, Guid folderId, Guid fileId)
    {
        throw new NotImplementedException();
    }

    public Task<Stream?> GetFileAsync(Guid userId, Guid folderId, Guid fileId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MoveFileAsync(Guid userId, Guid sourceFolderId, Guid destinationFolderId, Guid fileId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteFileAsync(Guid userId, Guid folderId, Guid fileId)
    {
        throw new NotImplementedException();
    }
}