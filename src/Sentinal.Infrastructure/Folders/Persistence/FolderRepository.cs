using Sentinal.Application.Common.Interfaces;
using Sentinal.Domain.Folders;
using Sentinal.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Sentinal.Infrastructure.Folders.Persistence;

public class FolderRepository : IFolderRepository
{
    private readonly SentinalDbContext _context;

    public FolderRepository(SentinalDbContext context)
    {
        _context = context;
    }


    public async Task<FolderEntity> CreateFolderAsync(string name, Guid userId, Guid? parentId = null)
    {
        var folder = new FolderEntity
        {
            FolderName = name,
            UserId = userId,
            ParentFolderId = parentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var newEntity = await _context.Folders.AddAsync(folder);
        if (await _context.SaveChangesAsync() > 0)
            return newEntity.Entity;
        throw new InvalidOperationException("Failed to create folder");
    }

    #region GetGroup

    public async Task<List<FolderEntity>> GetAllFoldersAsync(Guid userId)
    {
        return await _context.Folders
            .Where(f => f.UserId == userId && !f.MarkedForDeletion)
            .ToListAsync();
    }

    public async Task<FolderEntity?> GetFolderAsync(Guid id, Guid userId)
    {
        return await _context.Folders
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId && !f.MarkedForDeletion);
    }

    public async Task<List<FolderEntity>> GetFoldersMarkedForDeletionAsync(Guid userId)
    {
        return await _context.Folders
            .Where(f => f.MarkedForDeletion && f.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<FolderEntity>> GetFolderByNameAsync(string name, Guid userId)
    {
        return await _context.Folders
            .Where(f => EF.Functions.Like(f.FolderName.ToLower(), $"%{name.ToLower()}%") && f.UserId == userId && !f.MarkedForDeletion)
            .ToListAsync();
    }


    public async Task<List<FolderEntity>> GetFoldersSubFoldersAsync(Guid folderId, Guid userId)
    {
        return await _context.Folders
            .Where(f => f.ParentFolderId == folderId && f.UserId == userId && !f.MarkedForDeletion)
            .ToListAsync();
    }

    #endregion

    #region Existence Checks

    public async Task<bool> CheckIfFolderExistsUnderParentAsync(string name, Guid? parentFolderId, Guid userId)
    {
        return await _context.Folders
            .AnyAsync(f => f.FolderName == name
                && f.ParentFolderId == parentFolderId
                && f.UserId == userId
                && !f.MarkedForDeletion);
    }

    public async Task<bool> CheckIfFolderExistsAsync(Guid id, Guid userId)
    {
        return await _context.Folders
            .AnyAsync(f => f.Id == id && f.UserId == userId && !f.MarkedForDeletion);
    }

    #endregion

    #region UpdateGroup

    public async Task<bool> UpdateFolderNameAsync(Guid folderId, string newName, Guid userId)
    {
        var folder = await _context.Folders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);

        if (folder == null)
            throw new InvalidOperationException("Folder not found or user does not own this folder");

        var folderExists = await _context.Folders
            .AnyAsync(f => f.FolderName == newName
                && f.ParentFolderId == folder.ParentFolderId
                && f.Id != folderId
                && f.UserId == userId
                && !f.MarkedForDeletion);

        if (folderExists)
            throw new InvalidOperationException("A folder with this name already exists in the parent directory");

        folder.FolderName = newName;
        folder.UpdatedAt = DateTime.UtcNow;
        _context.Folders.Update(folder);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> MoveFolderAsync(Guid sourceFolderId, Guid destinationFolderId, Guid userId)
    {
        var sourceFolder = await _context.Folders
            .FirstOrDefaultAsync(f => f.Id == sourceFolderId && f.UserId == userId && !f.MarkedForDeletion);

        if (sourceFolder == null)
            throw new InvalidOperationException("Source folder not found or user does not own it");

        var destFolder = await _context.Folders
            .FirstOrDefaultAsync(f => f.Id == destinationFolderId && f.UserId == userId && !f.MarkedForDeletion);

        if (destFolder == null)
            throw new InvalidOperationException("Destination folder not found or user does not own it");

        var folderExists = await _context.Folders
            .AnyAsync(f => f.FolderName == sourceFolder.FolderName
                && f.ParentFolderId == destinationFolderId
                && f.Id != sourceFolderId
                && f.UserId == userId
                && !f.MarkedForDeletion);

        if (folderExists)
            throw new InvalidOperationException("A folder with this name already exists in the destination directory");

        sourceFolder.ParentFolderId = destinationFolderId;
        sourceFolder.UpdatedAt = DateTime.UtcNow;
        _context.Folders.Update(sourceFolder);
        return await _context.SaveChangesAsync() > 0;
    }

    #endregion

    #region DeleteGroup

    public async Task<bool> MarkFolderAsDeletedAsync(Guid folderId, Guid userId)
    {
        var folder = await _context.Folders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);

        if (folder == null)
            throw new InvalidOperationException("Folder not found or user does not own this folder");

        folder.MarkedForDeletion = true;
        folder.UpdatedAt = DateTime.UtcNow;
        folder.DeletedAt = DateTime.UtcNow;

        _context.Folders.Update(folder);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteFolderPermanentlyAsync(Guid folderId)
    {
        var folder = await _context.Folders.FirstOrDefaultAsync(f => f.Id == folderId);

        if (folder == null)
            return false;

        _context.Folders.Remove(folder);
        return await _context.SaveChangesAsync() > 0;
    }

    #endregion
}
