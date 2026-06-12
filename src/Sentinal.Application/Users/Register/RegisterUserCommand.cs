using FluentResults;
using MediatR;
using Sentinal.Application.Users.DTOs;
using Sentinal.Domain.Users;

namespace Sentinal.Application.Users.Register;

public record RegisterUserCommand(string Username, string Email, string Password): IRequest<Result<UserAuthDto>>;