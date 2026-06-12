using FluentResults;
using MediatR;

namespace Sentinal.Application.Files.Delete;

public record DeleteFileCommand(Guid FileId, Guid UserId) : IRequest<Result<bool>>;