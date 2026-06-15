using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Users.DTOs;
using Sentinal.Domain.Folders;

namespace Sentinal.Application.Users.UpdateUsername;

public class UpdateUsernameCommandHandler : IRequestHandler<UpdateUsernameCommand, Result<UserAuthDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<UpdateUsernameCommandHandler> _logger;

    public UpdateUsernameCommandHandler(IUserRepository userRepository, IFolderRepository folderRepository, IJwtTokenService jwtTokenService, ILogger<UpdateUsernameCommandHandler> logger)
    {
        _userRepository = userRepository;
        _folderRepository = folderRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<UserAuthDto>> Handle(UpdateUsernameCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.NewUsername))
            return Result.Fail("Username is required");
        if (command.NewUsername.Length < 3 || command.NewUsername.Length > 255)
            return Result.Fail("Username must be between 3 and 255 characters");

        var user = await _userRepository.GetUserByIdAsync(command.UserId);
        if (user == null || user.MarkedForDeletion)
            return Result.Fail("User not found");

        if (await _userRepository.UsernameExistsAsync(command.NewUsername))
            return Result.Fail("Username already in use");

        user.Username = command.NewUsername;
        await _userRepository.UpdateUserDataAsync(user);

        // Root folder's display name mirrors the username and shares its Id (FolderEntity.Id == UserEntity.Id)
        await _folderRepository.UpdateFolderNameAsync(user.Id, user.Username, user.Id);

        // Special folders' names are derived from username too, keep them in sync
        var recycleBinId = await _folderRepository.GetRecyclingFolderIdAsync(user.Id);
        await _folderRepository.UpdateFolderNameAsync(recycleBinId, user.Username + "_" + SpecialFolderTypes.RecycleBin, user.Id);

        var historyId = await _folderRepository.GetHistoryFolderIdAsync(user.Id);
        await _folderRepository.UpdateFolderNameAsync(historyId, user.Username + "_" + SpecialFolderTypes.History, user.Id);

        var token = _jwtTokenService.GenerateToken(user);
        _logger.LogInformation("Username updated for user: {UserId}", user.Id);
        return Result.Ok(new UserAuthDto(user.Id, user.Username, user.Email, token));
    }
}