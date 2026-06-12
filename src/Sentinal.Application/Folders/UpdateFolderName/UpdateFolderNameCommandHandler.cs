using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.Update;

public class UpdateFolderNameCommandHandler : IRequestHandler<UpdateFolderNameCommand, Result<FolderDto>>
{
    private readonly ILogger<UpdateFolderNameCommandHandler> _logger;
    private readonly IFolderRepository _folderRepository;

    public UpdateFolderNameCommandHandler(ILogger<UpdateFolderNameCommandHandler> logger, IFolderRepository folderRepository)
    {
        _logger = logger;
        _folderRepository = folderRepository;
    }

    public async Task<Result<FolderDto>> Handle(UpdateFolderNameCommand command, CancellationToken cancellationToken)
    {
        if (command.FolderId == Guid.Empty)
            return Result.Fail("Folder id cannot be empty");
        if (string.IsNullOrWhiteSpace(command.NewName))
            return Result.Fail("Folder name cannot be empty");
        if (command.NewName.Length > 255)
            return Result.Fail("Folder name cannot be longer than 255 characters");
        if (command.UserId == Guid.Empty)
            return Result.Fail("User id cannot be empty");

        try
        {

            _logger.LogInformation("Updating folder name. Folder Id: {FolderId}", command.FolderId);
            var result = await _folderRepository.UpdateFolderNameAsync(command.FolderId, command.NewName, command.UserId);
            if (result)
            {
                _logger.LogInformation("Folder name updated successfully: {FolderId}", command.FolderId);
                return Result.Ok(new FolderDto(command.FolderId, command.NewName, null, null, null));
            }
            return Result.Fail("Failed to update folder name");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating folder name. Folder Id: {FolderId}", command.FolderId);
            return Result.Fail("Failed to update folder name. Please try again later.");
        }
    }
}
