using FluentResults;

namespace Sentinal.Application.Common.Interfaces;

/// <summary>
/// Abstraction for file storage operations across multiple storage providers.
/// Storage is flat per-user: /{userId}/{fileId}. Folder hierarchy is virtual and lives only in the database.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to the configured storage provider.
    /// </summary>
    Task<bool> SaveFileAsync(Guid userId, Guid fileId, Stream fileContent);

    /// <summary>
    /// Checks if a file exists in storage.
    /// </summary>
    Task<bool> FileExistsAsync(Guid userId, Guid fileId);

    /// <summary>
    /// Retrieves a file from storage as a stream.
    /// </summary>
    Task<Stream> GetFileAsync(Guid userId, Guid fileId);

    /// <summary>
    /// Creates a user's root storage location when they first sign up.
    /// </summary>
    Task<bool> CreateRootFolderAsync(Guid userId);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    Task<bool> DeleteFileAsync(Guid userId, Guid fileId);
}
