using FluentResults;
using MediatR;

namespace Sentinal.Application.Files.UpdateFileDescription;

public record UpdateFileDescriptionCommand(Guid FileId, Guid UserId, string NewDescription) : IRequest<Result<bool>>;