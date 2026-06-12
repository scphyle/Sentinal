using FluentResults;
using MediatR;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.Move;

public record MoveFolderCommand(Guid SourceFolderId, Guid DestinationFolderId, Guid UserId) : IRequest<Result<FolderDto>>;
