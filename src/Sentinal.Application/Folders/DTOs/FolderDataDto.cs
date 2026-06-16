namespace Sentinal.Application.Folders.DTOs;

public record FolderDataDto(
    Guid FolderId,
    string FolderName,
    Guid? ParentFolderId,
    DateTime DateCreated,
    DateTime DateUpdated);
