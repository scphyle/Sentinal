using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Users.DTOs;
using Sentinal.Domain.Users;
using Sentinal.Infrastructure.Common.Security;

namespace Sentinal.Application.Users.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserAuthDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<RegisterUserCommandHandler> logger, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<UserAuthDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password) || string.IsNullOrWhiteSpace(command.Email))
            return Result.Fail("Username, password and email are required");
        if(command.Username.Length < 3 || command.Username.Length > 255)
            return Result.Fail("Username must be between 3 and 255 characters");
        if(command.Email.Length < 7 || command.Email.Length > 255)
            return Result.Fail("Email must be between 7 and 255 characters");

        try
        {
            var hashedPassword = _passwordHasher.HashPassword(command.Password);
            var user = await _userRepository.CreateUserAsync(command.Username, command.Email, hashedPassword);
            var token = _jwtTokenService.GenerateToken(user);

            return Result.Ok(new UserAuthDto(user.Id, user.Username, user.Email, token));
        }
        catch(InvalidOperationException e)
        {
            _logger.LogError(e, "Error creating user with username {Username}", command.Username);
            if(e.Message.Contains("duplicate email"))
                return Result.Fail("Email already exists");
            return Result.Fail(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating user with username {Username}", command.Username);
            return Result.Fail("Failed to create user. Please try again later.");
        }

    }
}