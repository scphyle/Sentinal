namespace Sentinal.Application.Folders.DTOs;

public record FolderDto(
    Guid Id,
    string Name,
    Guid? ParentFolderId,
    DateTime CreatedAt,
    DateTime UpdatedAt);