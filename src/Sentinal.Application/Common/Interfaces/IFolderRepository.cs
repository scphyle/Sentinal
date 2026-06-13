using Sentinal.Domain.Files;
using Sentinal.Domain.Folders;

namespace Sentinal.Application.Common.Interfaces;

public interface IFolderRepository
{
    Task<List<FolderEntity>> GetAllFoldersAsync(Guid userId);
    Task<FolderEntity?> GetFolderAsync(Guid id, Guid userId);
    Task<List<FolderEntity>> GetFolderByNameAsync(string name, Guid userId);
    Task<bool> CheckIfFolderExistsUnderParentAsync(string name, Guid? parentFolderId, Guid userId);
    Task<List<FolderEntity>> GetFoldersSubFoldersAsync(Guid folderId, Guid userId);
    Task<bool> MarkFolderAsDeletedAsync(Guid folderId, Guid userId);
    Task<List<FolderEntity>> GetFoldersMarkedForDeletionAsync(Guid userId);
    Task<FolderEntity> CreateFolderAsync(string name, Guid userId, Guid? parentId = null);
    Task<bool> MoveFolderAsync(Guid sourceFolderId, Guid destinationFolderId, Guid userId);
    Task<bool> UpdateFolderNameAsync(Guid folderId, string newName, Guid userId);
    Task<bool> CheckIfFolderExistsAsync(Guid id, Guid userId);
    Task<Guid> GetRecyclingFolderIdAsync(Guid userId);
    Task<Guid> GetHistoryFolderIdAsync(Guid userId);

}