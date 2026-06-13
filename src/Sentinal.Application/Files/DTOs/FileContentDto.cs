namespace Sentinal.Application.Files.DTOs;

public record FileContentDto
(
    string FileName,
    string ContentType,
    Guid FileId,
    Stream FileStream
);