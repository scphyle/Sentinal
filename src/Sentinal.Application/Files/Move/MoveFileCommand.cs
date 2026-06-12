using FluentResults;
using MediatR;

namespace Sentinal.Application.Files.Move;

public record MoveFileCommand(Guid FileId, Guid DestinationFolderId, Guid UserId) : IRequest<Result<bool>>;