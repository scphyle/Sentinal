using FluentResults;
using MediatR;
using Sentinal.Application.Users.DTOs;

namespace Sentinal.Application.Users.UpdateUsername;

public record UpdateUsernameCommand(Guid UserId, string NewUsername) : IRequest<Result<UserAuthDto>>;