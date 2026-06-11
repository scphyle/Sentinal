using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Domain.Users;
using Sentinal.Infrastructure.Common.Security;

namespace Sentinal.Application.Users.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<RegisterUserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<RegisterUserDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password) || string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Fail("Username, password and email are required");
        }
        if(await _userRepository.EmailExistsAsync(command.Email))
            return Result.Fail("Email already exists");
        if(await _userRepository.UsernameExistsAsync(command.Username))
            return Result.Fail("Username already exists");

        var hashedPassword = _passwordHasher.HashPassword(command.Password);
        try
        {
            var user = await _userRepository.CreateUserAsync(command.Username, command.Email, hashedPassword);
            return Result.Ok(new RegisterUserDto(user.Id, user.Username, user.Email));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating user with username {Username}", command.Username);
            return Result.Fail(e.Message);
        }

    }
}