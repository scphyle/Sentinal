using FluentResults;
using MediatR;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.GetAllFolders;

public record GetAllFoldersQuery(Guid UserId) : IRequest<Result<List<FolderDto>>>;
