using FluentResults;
using MediatR;

namespace Sentinal.Application.FIles.Create;

public record CreateFileCommand(string Name,
    string ContentType,
    MemoryStream Stream,
    Guid UserId,
    Guid FolderId,
    string? Description = null) : IRequest<Result<Guid>>;