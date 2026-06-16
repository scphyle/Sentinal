using Sentinal.Domain.Users;
using Sentinal.Domain.Folders;
using Sentinal.Domain.Files;

namespace Sentinal.Tests.Helpers;

public static class TestDataBuilder
{
    public static UserEntity CreateTestUser(
        string username = "testuser",
        string email = "test@example.com",
        string passwordHash = "hashedpassword")
    {
        return new UserEntity
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailConfirmed = false,
            MarkedForDeletion = false
        };
    }

    public static FolderEntity CreateTestFolder(
        string name = "Test Folder",
        Guid? userId = null,
        Guid? parentId = null,
        SpecialFolderTypes? folderType = null,
        Guid? id = null,
        bool markedForDeletion = false)
    {
        userId ??= Guid.NewGuid();
        return new FolderEntity
        {
            Id = id ?? Guid.NewGuid(),
            FolderName = name,
            UserId = userId.Value,
            ParentFolderId = parentId,
            FolderType = folderType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            MarkedForDeletion = markedForDeletion,
            DeletedAt = markedForDeletion ? DateTime.UtcNow : null
        };
    }

    public static FileEntity CreateTestFile(
        string fileName = "test.txt",
        Guid? userId = null,
        Guid? folderId = null,
        long fileSize = 1024,
        string contentType = "text/plain",
        string description = "Test file",
        Guid? id = null,
        bool markedForDeletion = false)
    {
        userId ??= Guid.NewGuid();
        folderId ??= Guid.NewGuid();

        return new FileEntity
        {
            Id = id ?? Guid.NewGuid(),
            FileName = fileName,
            FileSize = fileSize,
            ContentType = contentType,
            UserId = userId.Value,
            FolderId = folderId.Value,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            MarkedForDeletion = markedForDeletion,
            IsPartOfHistory = false,
            DeletedAt = markedForDeletion ? DateTime.UtcNow : null
        };
    }
}
