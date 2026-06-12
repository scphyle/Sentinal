using FluentResults;
using MediatR;
using Sentinal.Application.Users.DTOs;

namespace Sentinal.Application.Users.Login;

public record LoginUserCommand(string Password, string? Username, string? Email) : IRequest<Result<UserAuthDto>>;
