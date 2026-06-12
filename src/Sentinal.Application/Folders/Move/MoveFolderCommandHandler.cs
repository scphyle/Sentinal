using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.Move;

public class MoveFolderCommandHandler : IRequestHandler<MoveFolderCommand, Result<FolderDto>>
{
    private readonly ILogger<MoveFolderCommandHandler> _logger;
    private readonly IFolderRepository _folderRepository;

    public MoveFolderCommandHandler(ILogger<MoveFolderCommandHandler> logger, IFolderRepository folderRepository)
    {
        _logger = logger;
        _folderRepository = folderRepository;
    }

    public async Task<Result<FolderDto>> Handle(MoveFolderCommand command, CancellationToken cancellationToken)
    {
        if (command.SourceFolderId == Guid.Empty)
            return Result.Fail("Source folder id cannot be empty");
        if (command.DestinationFolderId == Guid.Empty)
            return Result.Fail("Destination folder id cannot be empty");
        if (command.UserId == Guid.Empty)
            return Result.Fail("User id cannot be empty");
        if (command.SourceFolderId == command.DestinationFolderId)
            return Result.Fail("Source and destination folders cannot be the same");

        _logger.LogInformation("Moving folder {SourceFolderId} to {DestinationFolderId}", command.SourceFolderId, command.DestinationFolderId);

        try
        {
            var result = await _folderRepository.MoveFolderAsync(command.SourceFolderId, command.DestinationFolderId, command.UserId);
            if (result)
            {
                _logger.LogInformation("Folder moved successfully: {SourceFolderId} to {DestinationFolderId}", command.SourceFolderId, command.DestinationFolderId);
                return Result.Ok(new FolderDto(command.SourceFolderId, null,command.DestinationFolderId, null, null));
            }
            return Result.Fail("Failed to move folder");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error moving folder. Source Id: {SourceFolderId}, Destination Id: {DestinationFolderId}", command.SourceFolderId, command.DestinationFolderId);
            return Result.Fail("Failed to move folder. Please try again later.");
        }
    }
}