using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;

namespace Sentinal.Application.Users.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(IUserRepository userRepository, ILogger<ConfirmEmailCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(command.UserId);
        if (user == null || user.MarkedForDeletion)
            return Result.Fail("User not found");

        if (user.EmailConfirmed)
            return Result.Ok(true);

        user.EmailConfirmed = true;
        await _userRepository.UpdateUserDataAsync(user);

        _logger.LogInformation("Email confirmed for user: {UserId}", user.Id);
        return Result.Ok(true);
    }
}
