using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.GetFoldersInRecycleBin;

public class GetFoldersInRecycleBinQueryHandler : IRequestHandler<GetFoldersInRecycleBinQuery, Result<List<FolderDto>>>
{
    private readonly ILogger<GetFoldersInRecycleBinQueryHandler> _logger;
    private readonly IFolderRepository _folderRepository;

    public GetFoldersInRecycleBinQueryHandler(ILogger<GetFoldersInRecycleBinQueryHandler> logger, IFolderRepository folderRepository)
    {
        _logger = logger;
        _folderRepository = folderRepository;
    }

    public async Task<Result<List<FolderDto>>> Handle(GetFoldersInRecycleBinQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
            return Result.Fail("User id cannot be empty");

        try
        {
            var folders = await _folderRepository.GetFoldersMarkedForDeletionAsync(query.UserId);
            if (folders == null || folders.Count == 0)
            {
                _logger.LogInformation("No deleted folders found for user: {UserId}", query.UserId);
                return Result.Ok(new List<FolderDto>());
            }

            var folderDtos = folders.Select(f => new FolderDto(f.Id, f.FolderName, f.ParentFolderId, f.CreatedAt, f.UpdatedAt)).ToList();
            _logger.LogInformation("Retrieved {Count} deleted folders for user: {UserId}", folderDtos.Count, query.UserId);
            return Result.Ok(folderDtos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving deleted folders. User Id: {UserId}", query.UserId);
            return Result.Fail("Failed to retrieve deleted folders. Please try again later.");
        }
    }
}
