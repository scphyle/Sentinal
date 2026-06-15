using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Common.Persistence;
using Sentinal.Domain.Files;
using Microsoft.EntityFrameworkCore;


namespace Sentinal.Infrastructure.Files.Persistence;

public class FileRepository : IFileRepository
{
    private readonly SentinalDbContext _context;
    private readonly IFolderRepository _folderRepository;
    public FileRepository(SentinalDbContext context, IFolderRepository folderRepository)
    {
        _context = context;
        _folderRepository = folderRepository;
    }

    public async Task<FileEntity> CreateFileAsync(string fileName,
        long fileSize,
        string contentType,
        Guid userId,
        Guid folderId,
        string? description = null)
    {
        var folder = await _folderRepository.GetFolderAsync(folderId, userId);
        if (folder == null)
            throw new Exception("Folder does not exist");
        if (folder.Files.Any(x => x.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
            throw new Exception("File already exists");

        var newFile = new FileEntity()
        {
            FileName = fileName,
            FileSize = fileSize,
            ContentType = contentType,
            UserId = userId,
            FolderId = folderId,
            Description = description ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Files.AddAsync(newFile);
        await _context.SaveChangesAsync();

        return newFile;
    }

    public async Task<List<FileEntity>> GetAllUserFilesAsync(Guid userId)
    {
        return await _context.Files.Where(x => x.UserId == userId && 
                                               !x.MarkedForDeletion &&
                                               !x.IsPartOfHistory).ToListAsync();
    }

    //TODO: Figure out if we should allow users to search for files marked as deleted and ones that are part of history
    public async Task<FileEntity?> GetFileAsync(Guid fileId, Guid userId)
    {
        return await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId &&
                                                             x.UserId == userId );
    }

    public async Task<List<FileEntity>> GetFileHistory(Guid fileId, Guid userId)
    {
        var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId && x.UserId == userId);
        if (file == null)
            throw new InvalidOperationException("File not found");

        var history = new List<FileEntity> { file };
        while (file.PreviousVersionId is Guid previousId)
        {
            file = await _context.Files.FirstOrDefaultAsync(x => x.Id == previousId && x.UserId == userId);
            if (file == null) break;
            history.Add(file);
        }
        return history;
    }



    public async Task<List<FileEntity>> GetFilesByFolderIdAsync(Guid folderId, Guid userId)
    {
        var folder = await _folderRepository.GetFolderAsync(folderId, userId);
        if (folder == null)
            throw new InvalidDataException("Folder does not exist");
        return await _context.Files.Where(x => x.FolderId == folderId &&
                                                    !x.MarkedForDeletion &&
                                                    !x.IsPartOfHistory).ToListAsync();
    }

    public async Task<List<FileEntity>> GetFilesMarkedForDeletionAsync(Guid userId)
    {
        return await _context.Files.Where(x => x.MarkedForDeletion && 
                                               x.UserId == userId).ToListAsync();
    }

    public async Task<bool> FileExistsAsync(Guid fileId, Guid userId)
    {
        return await _context.Files.AnyAsync(x => x.Id == fileId &&
                                                  x.UserId == userId &&
                                                  !x.MarkedForDeletion);
    }

    public async Task<bool> MarkFileAsDeletedAsync(Guid fileId, Guid userId)
    {
        var file = _context.Files.FirstOrDefault(x => x.Id == fileId && x.UserId == userId);
        if (file == null)
            throw new InvalidOperationException("File not found");
        file.MarkedForDeletion = true;
        file.UpdatedAt = DateTime.UtcNow;
        file.DeletedAt = DateTime.UtcNow;
        _context.Files.Update(file);
        return await _context.SaveChangesAsync() > 0;

    }

    public async Task<bool> MoveFileAsync(Guid fileId, Guid destinationFolderId, Guid userId)
    {
        var newFolder = await _folderRepository.GetFolderAsync(destinationFolderId, userId);
        if (newFolder == null)
            throw new InvalidOperationException("Destination folder not found");
        var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId &&
                                                                 x.UserId == userId &&
                                                                 !x.IsPartOfHistory);
        if (file == null)
            throw new InvalidOperationException("File not found");
        file.FolderId = destinationFolderId;
        file.UpdatedAt = DateTime.UtcNow;
        _context.Files.Update(file);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<(FileEntity newFile,FileEntity oldFile)> UpdateFileContentAsync(Guid fileId, Guid userId, long fileSize, string? description = null)
    {
        var file = await  _context.Files.FirstOrDefaultAsync(x => x.Id == fileId && x.UserId == userId);
        if (file == null)
            throw new InvalidOperationException("File not found");
        
        var newFile = file.Copy();
        file.IsPartOfHistory = true;
        file.UpdatedAt = DateTime.UtcNow;
        file.FolderId = await _folderRepository.GetRecyclingFolderIdAsync(userId);

        newFile.PreviousVersionId = file.Id;
        if (description != null)
            newFile.Description = description;
        newFile.UpdatedAt = DateTime.UtcNow;
        newFile.FileSize = fileSize;
        await _context.Files.AddAsync(newFile);
        await _context.SaveChangesAsync();
        return (newFile,file);
        
    }
    
    public async Task<bool> UpdateFileNameAsync(Guid id, string newName, Guid userId)
    {
        var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        if (file == null)
            throw new InvalidOperationException("File not found");
        file.FileName = newName;
        file.UpdatedAt = DateTime.UtcNow;
        _context.Files.Update(file);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateFileDescriptionAsync(Guid id, string newDescription, Guid userId)
    {
        var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        if (file == null)
            throw new InvalidOperationException("File not found");
        file.Description = newDescription;
        file.UpdatedAt = DateTime.UtcNow;
        _context.Files.Update(file);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<FileEntity>> SearchFilesByNameAsync(string fileName, Guid userId)
    {
        return await _context.Files
            .Where(f => EF.Functions.Like(f.FileName.ToLower(), $"%{fileName.ToLower()}%") &&
                        f.UserId == userId && !f.MarkedForDeletion)
            .ToListAsync();
    }

    public async Task<bool> PermanentlyDeleteFileAsync(Guid id, Guid userId)
    {
        var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

        if (file == null)
            return false;

        _context.Files.Remove(file);
        return await _context.SaveChangesAsync() > 0;
    }
}