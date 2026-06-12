using FluentResults;
using MediatR;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.Create;

public record CreateFolderCommand(string Name, Guid UserId, Guid? ParentId = null) : IRequest<Result<CreateFolderDto>>;