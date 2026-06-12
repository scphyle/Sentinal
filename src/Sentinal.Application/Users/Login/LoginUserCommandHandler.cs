using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Users.DTOs;
using Sentinal.Domain.Users;
using Sentinal.Infrastructure.Common.Security;

namespace Sentinal.Application.Users.Login;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<UserAuthDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginUserCommandHandler> _logger;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<LoginUserCommandHandler> logger, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<UserAuthDto>> Handle(LoginUserCommand loginRequest, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(loginRequest.Email) && string.IsNullOrEmpty(loginRequest.Username))
            return Result.Fail("Email or Username is required");

        if(string.IsNullOrEmpty(loginRequest.Password))
            return Result.Fail("Password is required");

        UserEntity? user = null;
        if (!string.IsNullOrWhiteSpace(loginRequest.Email))
        {
            user = await _userRepository.GetUserByEmailAsync(loginRequest.Email);
        }
        else
        {
            user = await _userRepository.GetUserByUsernameAsync(loginRequest.Username);
        }
        if(user == null)
        {
            _logger.LogWarning("Login attempt with non-existent user: {Identifier}", loginRequest.Email ?? loginRequest.Username);
            return Result.Fail("User not found");
        }
        if(user.MarkedForDeletion)
        {
            _logger.LogWarning("Login attempt for soft-deleted user: {UserId}", user.Id);
            return Result.Fail("User not found");
        }

        if(_passwordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash))
        {
            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);
            var token = _jwtTokenService.GenerateToken(user);
            return Result.Ok(new UserAuthDto(user.Id, user.Username, user.Email, token));
        }

        _logger.LogWarning("Failed login attempt for user: {UserId}", user.Id);
        return Result.Fail("Invalid login attempt");
    }
}