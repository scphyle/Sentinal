using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Folders.DTOs;
using Sentinal.Domain.Folders;

namespace Sentinal.Application.Folders.Create;

public class CreateFolderCommandHandler :  IRequestHandler<CreateFolderCommand, Result<CreateFolderDto>>
{
    private ILogger<CreateFolderCommandHandler> _logger;
    private readonly IFolderRepository _folderRepository;
    public CreateFolderCommandHandler(ILogger<CreateFolderCommandHandler> logger, IFolderRepository folderRepository)
    {
        _logger = logger;
        _folderRepository = folderRepository;
    }

    public async Task<Result<CreateFolderDto>> Handle(CreateFolderCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result.Fail("Folder name cannot be empty");
        if (command.Name.Length > 255)
            return Result.Fail("Folder name cannot be longer than 255 characters");
        if (command.UserId == Guid.Empty)
            return Result.Fail("User id cannot be empty");
        if(command.ParentId.HasValue && command.ParentId.Value == Guid.Empty)
            return Result.Fail("Parent folder cannot be empty");

        if (await _folderRepository.CheckIfFolderExistsUnderParentAsync(command.Name, command.ParentId, command.UserId))
            return Result.Fail("A folder with this name already exists under the parent directory");

        if(command.ParentId.HasValue && command.ParentId.Value != Guid.Empty)
            if(!await _folderRepository.CheckIfFolderExistsAsync(command.ParentId.Value, command.UserId))
                return Result.Fail("Parent folder does not exist");

        _logger.LogInformation("Creating folder {Name}", command.Name);

        try
        {
           var newFolder =  await _folderRepository.CreateFolderAsync(command.Name, command.UserId, command.ParentId);
            return Result.Ok(new CreateFolderDto(newFolder.Id, newFolder.FolderName, newFolder.ParentFolderId));
        }catch(Exception e)
        {
            _logger.LogError(e, "Error creating folder {Name}", command.Name);
            return Result.Fail("Failed to create folder. Please try again later.");
        }

    }
}