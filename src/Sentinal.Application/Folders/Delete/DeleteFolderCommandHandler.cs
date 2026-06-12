using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.Delete;

public class DeleteFolderCommandHandler : IRequestHandler<DeleteFolderCommand, Result<UpdateFolderDto>>
{
    private readonly ILogger<DeleteFolderCommandHandler> _logger;
    private readonly IFolderRepository _folderRepository;

    public DeleteFolderCommandHandler(ILogger<DeleteFolderCommandHandler> logger, IFolderRepository folderRepository)
    {
        _logger = logger;
        _folderRepository = folderRepository;
    }

    public async Task<Result<UpdateFolderDto>> Handle(DeleteFolderCommand command, CancellationToken cancellationToken)
    {
        if (command.FolderId == Guid.Empty)
            return Result.Fail("Folder id cannot be empty");
        if (command.UserId == Guid.Empty)
            return Result.Fail("User id cannot be empty");

        var folder = await _folderRepository.GetFolderAsync(command.FolderId, command.UserId);
        if (folder == null)
            return Result.Fail("Folder not found or does not belong to you");

        _logger.LogInformation("Deleting folder {FolderId}", command.FolderId);

        try
        {
            var result = await _folderRepository.MarkFolderAsDeletedAsync(command.FolderId, command.UserId);
            if (result)
            {
                _logger.LogInformation("Folder deleted successfully: {FolderId}", command.FolderId);
                return Result.Ok(new UpdateFolderDto(folder.Id, folder.FolderName));
            }
            return Result.Fail("Failed to delete folder");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting folder. Folder Id: {FolderId}", command.FolderId);
            return Result.Fail("Failed to delete folder. Please try again later.");
        }
    }
}