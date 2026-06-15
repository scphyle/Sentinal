using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Users.DTOs;

namespace Sentinal.Application.Users.UpdateEmail;

public class UpdateUserEmailCommandHandler : IRequestHandler<UpdateUserEmailCommand, Result<UserAuthDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<UpdateUserEmailCommandHandler> _logger;

    public UpdateUserEmailCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService, ILogger<UpdateUserEmailCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<UserAuthDto>> Handle(UpdateUserEmailCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.NewEmail) || !command.NewEmail.Contains('@'))
            return Result.Fail("A valid email is required");
        if (command.NewEmail.Length < 7 || command.NewEmail.Length > 255)
            return Result.Fail("Email must be between 7 and 255 characters");

        var user = await _userRepository.GetUserByIdAsync(command.UserId);
        if (user == null || user.MarkedForDeletion)
            return Result.Fail("User not found");

        if (await _userRepository.EmailExistsAsync(command.NewEmail))
            return Result.Fail("Email already in use");

        user.Email = command.NewEmail;
        user.EmailConfirmed = false;
        await _userRepository.UpdateUserDataAsync(user);

        var token = _jwtTokenService.GenerateToken(user);
        _logger.LogInformation("Email updated for user: {UserId}", user.Id);
        return Result.Ok(new UserAuthDto(user.Id, user.Username, user.Email, token));
    }
}