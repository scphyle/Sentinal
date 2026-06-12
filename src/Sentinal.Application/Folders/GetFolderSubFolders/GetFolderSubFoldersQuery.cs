using FluentResults;
using MediatR;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.GetFolderSubFolders;

public record GetFolderSubFoldersQuery(Guid FolderId, Guid UserId) : IRequest<Result<List<FolderDto>>>;
