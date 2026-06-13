using FluentResults;
using MediatR;
using Sentinal.Application.Files.DTOs;

namespace Sentinal.Application.FIles.GetAllFilesInAFolder;

public record GetAllFilesInFolderQuery(Guid FolderId, Guid UserId):IRequest<Result<List<FileDataDto>>>;