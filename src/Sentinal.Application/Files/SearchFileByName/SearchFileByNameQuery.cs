using FluentResults;
using MediatR;
using Sentinal.Application.Files.DTOs;

namespace Sentinal.Application.Files.SearchFileByName;

public record SearchFileByNameQuery(string FileName, Guid UserId): IRequest<Result<List<FileDataDto>>>;