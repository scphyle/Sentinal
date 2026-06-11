using Sentinal.Domain.Files;

namespace Sentinal.Application.Common.Interfaces;

public interface IFileRepository
{
    Task<IEnumerable<FileEntity>> GetFiles();
    Task<FileEntity?> GetFile(Guid id);
    Task<int> SaveChangesAsync();

}