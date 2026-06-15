using Microsoft.Extensions.Options;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Options;

namespace Sentinal.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _config;
    public LocalFileStorageService(IOptions<FileStorageOptions> config)
    {
        _config = config.Value;
    }

    private string GetUserPath(Guid userId) => Path.Combine(_config.BasePath, userId.ToString());

    private string GetFilePath(Guid userId, Guid fileId) => Path.Combine(GetUserPath(userId), fileId.ToString());

    public async Task<bool> SaveFileAsync(Guid userId, Guid fileId, Stream fileContent)
    {
        Directory.CreateDirectory(GetUserPath(userId));

        await using var fileStream = new FileStream(GetFilePath(userId, fileId), FileMode.Create, FileAccess.Write);
        await fileContent.CopyToAsync(fileStream);
        return true;
    }

    public Task<bool> FileExistsAsync(Guid userId, Guid fileId)
    {
        return Task.FromResult(File.Exists(GetFilePath(userId, fileId)));
    }

    public Task<Stream> GetFileAsync(Guid userId, Guid fileId)
    {
        var path = GetFilePath(userId, fileId);
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found in storage", path);

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task<bool> CreateRootFolderAsync(Guid userId)
    {
        Directory.CreateDirectory(GetUserPath(userId));
        return Task.FromResult(true);
    }

    public Task<bool> DeleteFileAsync(Guid userId, Guid fileId)
    {
        var path = GetFilePath(userId, fileId);
        if (File.Exists(path))
            File.Delete(path);
        return Task.FromResult(true);
    }
}
