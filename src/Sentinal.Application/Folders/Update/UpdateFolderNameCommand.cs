using FluentResults;
using MediatR;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.Update;

public record UpdateFolderNameCommand(Guid FolderId, string NewName, Guid UserId) : IRequest<Result<FolderDto>>;
