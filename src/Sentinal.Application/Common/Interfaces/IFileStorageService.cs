using FluentResults;

namespace Sentinal.Application.Common.Interfaces;

/// <summary>
/// Abstraction for file storage operations across multiple storage providers.
/// Files are stored in a structure: /{userId}/{folderId}/{fileId}
/// This approach prevents naming conflicts and ensures security through GUID-based paths.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to the configured storage provider.
    /// </summary>
    Task<bool> SaveFileAsync(Guid userId, Guid folderId, Guid fileId, Stream fileContent);

    /// <summary>
    /// Checks if a file exists in storage.
    /// </summary>
    Task<bool> FileExistsAsync(Guid userId, Guid folderId, Guid fileId);

    /// <summary>
    /// Retrieves a file from storage as a stream.
    /// </summary>
    Task<Stream> GetFileAsync(Guid userId, Guid folderId, Guid fileId);

    /// <summary>
    /// Moves a file to a different folder.
    /// </summary>
    Task<bool> MoveFileAsync(Guid userId, Guid sourceFolderId, Guid destinationFolderId, Guid fileId);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    Task<bool> DeleteFileAsync(Guid userId, Guid folderId, Guid fileId);
}