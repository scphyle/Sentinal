using Sentinal.Domain.Files;
using Sentinal.Domain.Folders;

namespace Sentinal.Application.Folders.DTOs;

public record FolderDto(
    Guid Id,
    string? Name,
    Guid? ParentFolderId,
    DateTime? CreatedAt,
    DateTime? UpdatedAt,
    List<FolderEntity>? Children = null,
    List<FileEntity>? Files = null);