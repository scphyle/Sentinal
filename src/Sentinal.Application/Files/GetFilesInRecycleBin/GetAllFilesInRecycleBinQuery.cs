using FluentResults;
using MediatR;
using Sentinal.Application.Files.DTOs;

namespace Sentinal.Application.Files.GetFilesInRecycleBin;

public record GetAllFilesInRecycleBinQuery(Guid UserId): IRequest<Result<List<FileDataDto>>> ;