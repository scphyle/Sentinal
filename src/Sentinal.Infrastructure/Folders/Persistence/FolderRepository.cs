using Sentinal.Application.Common.Interfaces;
using Sentinal.Domain.Folders;
using Sentinal.Infrastructure.Common.Persistence;

namespace Sentinal.Infrastructure.Folders.Persistence;

public class FolderRepository : IFolderRepository
{
    private readonly SentinalDbContext _context;

    public FolderRepository(SentinalDbContext context)
    {
        _context = context;
    }

    public Task<IEnumerable<FolderEntity>> GetFolders()
    {
        throw new NotImplementedException();
    }

    public async Task<FolderEntity> GetFolder(Guid id)
    {
        //TODO: Convert to Fluent
        return await _context.Folders.FindAsync(id) ?? throw new Exception();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}