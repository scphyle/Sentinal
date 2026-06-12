namespace Sentinal.Application.Folders.DTOs;

public record CreateFolderDto(Guid FolderId, string Name, Guid? ParentId);