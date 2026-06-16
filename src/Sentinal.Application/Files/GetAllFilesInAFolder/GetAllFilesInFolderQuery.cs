using FluentResults;
using MediatR;
using Sentinal.Application.Files.DTOs;

namespace Sentinal.Application.Files.GetAllFilesInAFolder;

public record GetAllFilesInFolderQuery(Guid FolderId, Guid UserId):IRequest<Result<List<FileDataDto>>>;