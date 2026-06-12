using FluentResults;
using MediatR;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.GetFoldersInRecycleBin;

public record GetFoldersInRecycleBinQuery(Guid UserId) : IRequest<Result<List<FolderDto>>>;
