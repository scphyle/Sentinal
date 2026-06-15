using FluentResults;
using MediatR;
using Sentinal.Application.Users.DTOs;

namespace Sentinal.Application.Users.UpdateEmail;

public record UpdateUserEmailCommand(Guid UserId, string NewEmail) : IRequest<Result<UserAuthDto>>;