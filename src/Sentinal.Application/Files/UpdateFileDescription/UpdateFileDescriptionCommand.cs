using FluentResults;
using MediatR;

namespace Sentinal.Application.FIles.UpdateFileDescription;

public record UpdateFileDescriptionCommand(Guid FileId, Guid UserId, string NewDescription) : IRequest<Result<bool>>;