using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Common.Security;

namespace Sentinal.Application.Users.UpdatePassword;

public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UpdatePasswordCommandHandler> _logger;

    public UpdatePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<UpdatePasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdatePasswordCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CurrentPassword) || string.IsNullOrWhiteSpace(command.NewPassword))
            return Result.Fail("Current and new password are required");
        if (command.NewPassword.Length < 8 || command.NewPassword.Length > 255)
            return Result.Fail("New password must be between 8 and 255 characters");

        var user = await _userRepository.GetUserByIdAsync(command.UserId);
        if (user == null || user.MarkedForDeletion)
            return Result.Fail("User not found");

        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Failed password update attempt for user: {UserId}", user.Id);
            return Result.Fail("Current password is incorrect");
        }

        user.PasswordHash = _passwordHasher.HashPassword(command.NewPassword);
        await _userRepository.UpdateUserDataAsync(user);

        _logger.LogInformation("Password updated for user: {UserId}", user.Id);
        return Result.Ok(true);
    }
}
