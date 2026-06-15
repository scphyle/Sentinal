using FluentResults;
using MediatR;

namespace Sentinal.Application.Users.Delete;

public record DeleteUserCommand(Guid UserId) : IRequest<Result<bool>>;