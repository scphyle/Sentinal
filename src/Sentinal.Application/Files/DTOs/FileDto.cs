namespace Sentinal.Application.Files.DTOs;

public record FileDto(
    Guid Id,
    string FileName,
    long FileSize,
    string ContentType,
    string Description,
    Guid FolderId,
    Guid UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool MarkedForDeletion = false,
    DateTime? DeletedAt = null);
