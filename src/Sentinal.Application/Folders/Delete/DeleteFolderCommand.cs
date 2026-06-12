using FluentResults;
using MediatR;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.Delete;

public record DeleteFolderCommand(Guid FolderId, Guid UserId) : IRequest<Result<UpdateFolderDto>>;