using Microsoft.Extensions.Options;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Options;

namespace Sentinal.Infrastructure.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _config;
    public S3FileStorageService(IOptions<FileStorageOptions> config)
    {
        _config = config.Value;
    }

    public Task<bool> SaveFileAsync(Guid userId, Guid fileId, Stream fileContent)
    {
        throw new NotImplementedException();
    }

    public Task<bool> FileExistsAsync(Guid userId, Guid fileId)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> GetFileAsync(Guid userId, Guid fileId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateRootFolderAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteFileAsync(Guid userId, Guid fileId)
    {
        throw new NotImplementedException();
    }
}