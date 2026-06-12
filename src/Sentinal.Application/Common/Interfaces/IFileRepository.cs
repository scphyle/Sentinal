using Sentinal.Domain.Files;

namespace Sentinal.Application.Common.Interfaces;

public interface IFileRepository
{
    Task<FileEntity> CreateFileAsync(string fileName,
        long fileSize,
        string contentType,
        Guid userId,
        Guid folderId,
        string? description = null);
    Task<List<FileEntity>> GetFilesAsync(Guid userId);
    Task<FileEntity?> GetFileAsync(Guid fileId, Guid userId);
    Task<List<FileEntity>> GetFilesByFolderIdAsync(Guid folderId, Guid userId);
    Task<List<FileEntity>> GetFilesMarkedForDeletionAsync(Guid userId);
    Task<bool> FileExistsAsync(Guid fileId, Guid userId);
    Task<bool> MarkFileAsDeletedAsync(Guid fileId, Guid userId);
    Task<bool> MoveFileAsync(Guid fileId, Guid destinationFolderId,  Guid userId);
    Task<bool> UpdateFileNameAsync(Guid id, string newName, Guid userId);
    Task<bool> UpdateFileDescriptionAsync(Guid id, string newDescription, Guid userId);
    Task<List<FileEntity>> SearchFilesByNameAsync(string fileName, Guid userId);
    Task<bool> PermanentlyDeleteFileAsync(Guid id, Guid userId);

}