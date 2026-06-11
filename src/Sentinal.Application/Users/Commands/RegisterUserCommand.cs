using MediatR;
using Sentinal.Domain.Users;

namespace Sentinal.Application.Users.Commands;

public record RegisterUserCommand(string Username, string Email, string Password): IRequest<Guid>;