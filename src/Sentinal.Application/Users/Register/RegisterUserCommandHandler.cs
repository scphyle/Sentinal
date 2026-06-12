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
            var token = _jwtTokenService.GenerateToken(user);


            return Result.Ok(new UserAuthDto(user.Id, user.Username, user.Email, token));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating user with username {Username}", command.Username);
            return Result.Fail("Failed to create user. Please try again later.");
        }

    }
}