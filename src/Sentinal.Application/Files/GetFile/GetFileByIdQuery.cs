using FluentResults;
using MediatR;
using Sentinal.Application.Files.DTOs;

namespace Sentinal.Application.FIles.GetFile;

public record GetFileByIdQuery(Guid FileId, Guid UserId):IRequest<Result<FileContentDto>>;