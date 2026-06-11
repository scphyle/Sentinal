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
    /// <param name="userId">The ID of the file owner</param>
    /// <param name="folderId">The ID of the parent folder</param>
    /// <param name="fileId">The unique file identifier</param>
    /// <param name="fileContent">The file content stream</param>
    /// <returns>True if save was successful</returns>
    Task<bool> SaveFileAsync(Guid userId, Guid folderId, Guid fileId, Stream fileContent);

    /// <summary>
    /// Checks if a file exists in storage.
    /// </summary>
    /// <param name="userId">The ID of the file owner</param>
    /// <param name="folderId">The ID of the parent folder</param>
    /// <param name="fileId">The unique file identifier</param>
    /// <returns>True if the file exists</returns>
    Task<bool> FileExistsAsync(Guid userId, Guid folderId, Guid fileId);

    /// <summary>
    /// Retrieves a file from storage as a stream.
    /// </summary>
    /// <param name="userId">The ID of the file owner</param>
    /// <param name="folderId">The ID of the parent folder</param>
    /// <param name="fileId">The unique file identifier</param>
    /// <returns>A stream containing the file content, or null if file not found</returns>
    Task<Stream?> GetFileAsync(Guid userId, Guid folderId, Guid fileId);

    /// <summary>
    /// Moves a file to a different folder.
    /// </summary>
    /// <param name="userId">The ID of the file owner</param>
    /// <param name="sourceFolderId">The current parent folder ID</param>
    /// <param name="destinationFolderId">The target parent folder ID</param>
    /// <param name="fileId">The unique file identifier</param>
    /// <returns>True if move was successful</returns>
    Task<bool> MoveFileAsync(Guid userId, Guid sourceFolderId, Guid destinationFolderId, Guid fileId);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="userId">The ID of the file owner</param>
    /// <param name="folderId">The ID of the parent folder</param>
    /// <param name="fileId">The unique file identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteFileAsync(Guid userId, Guid folderId, Guid fileId);
}