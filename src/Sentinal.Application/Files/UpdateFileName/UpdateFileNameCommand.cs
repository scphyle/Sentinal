using FluentResults;
using MediatR;

namespace Sentinal.Application.FIles.UpdateFileName;

public record UpdateFileNameCommand(Guid FileId, Guid UserId, string NewFileName) : IRequest<Result<bool>>;