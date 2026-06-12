using FluentResults;
using MediatR;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.GetFolder;

public record GetFolderQuery(Guid Id, Guid UserId) : IRequest<Result<FolderDto>>;