using FluentResults;
using MediatR;
using Sentinal.Application.Files.DTOs;
using Sentinal.Domain.Files;

namespace Sentinal.Application.FIles.GetAllFiles;

public record GetAllFilesQuery(Guid UserId) : IRequest<Result<List<FileDataDto>>>;