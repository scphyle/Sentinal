using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Common.Persistence;
using Sentinal.Domain.Files;


namespace Sentinal.Infrastructure.Files.Persistence;

public class FileRepository : IFileRepository
{
    private readonly SentinalDbContext _context;
    public FileRepository(SentinalDbContext context)
    {
        _context = context;
    }
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<FileEntity>> GetFiles()
    {
        throw new NotImplementedException();
    }
    public async Task<FileEntity> GetFile(Guid id)
    {
        throw new NotImplementedException();
    }

}