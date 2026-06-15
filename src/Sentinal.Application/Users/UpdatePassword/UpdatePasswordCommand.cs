using FluentResults;
using MediatR;

namespace Sentinal.Application.Users.UpdatePassword;

public record UpdatePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Result<bool>>;