using FluentResults;
using MediatR;

namespace Sentinal.Application.Users.ConfirmEmail;

public record ConfirmEmailCommand(Guid UserId) : IRequest<Result<bool>>;