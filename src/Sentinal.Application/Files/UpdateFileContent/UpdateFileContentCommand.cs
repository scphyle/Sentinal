using FluentResults;
using MediatR;

namespace Sentinal.Application.Files.UpdateFileContent;

public record UpdateFileContentCommand(Guid FileId,
    Stream Stream,
    long FileSize,
    Guid UserId,
    String? Description = null) : IRequest<Result<Guid>>;