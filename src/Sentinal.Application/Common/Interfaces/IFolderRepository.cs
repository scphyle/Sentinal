using Sentinal.Domain.Folders;

namespace Sentinal.Application.Common.Interfaces;

public interface IFolderRepository
{
    Task<IEnumerable<FolderEntity>> GetFolders();
    Task<FolderEntity?> GetFolder(Guid id);
    Task<int> SaveChangesAsync();

}