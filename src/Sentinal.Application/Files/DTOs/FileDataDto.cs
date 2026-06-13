namespace Sentinal.Application.Files.DTOs;

public record FileDataDto(Guid FileId, 
    string FileName, 
    string FileType, 
    string? FileDescription, 
    DateTime DateCreated, 
    DateTime DateUpdated,
    Guid FolderId);