using FluentResults;
using MediatR;

namespace Sentinal.Application.Users.Login;

public record LoginUserCommand(string Password, string? Username, string? Email) : IRequest<Result<LoginUserDto>>;
