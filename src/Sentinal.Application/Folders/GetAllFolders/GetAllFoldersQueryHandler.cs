using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.GetAllFolders;

public class GetAllFoldersQueryHandler : IRequestHandler<GetAllFoldersQuery, Result<List<FolderDto>>>
{
    private readonly ILogger<GetAllFoldersQueryHandler> _logger;
    private readonly IFolderRepository _folderRepository;

    public GetAllFoldersQueryHandler(ILogger<GetAllFoldersQueryHandler> logger, IFolderRepository folderRepository)
    {
        _logger = logger;
        _folderRepository = folderRepository;
    }

    public async Task<Result<List<FolderDto>>> Handle(GetAllFoldersQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
            return Result.Fail("User id cannot be empty");

        try
        {
            var folders = await _folderRepository.GetAllFoldersAsync(query.UserId);
            if (folders == null || folders.Count == 0)
            {
                _logger.LogInformation("No folders found for user: {UserId}", query.UserId);
                return Result.Ok(new List<FolderDto>());
            }

            var folderDtos = folders.Select(f => new FolderDto(f.Id, f.FolderName,f.ParentFolderId ,f.CreatedAt, f.UpdatedAt)).ToList();
            _logger.LogInformation("Retrieved {Count} folders for user: {UserId}", folderDtos.Count, query.UserId);
            return Result.Ok(folderDtos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving folders for user. User Id: {UserId}", query.UserId);
            return Result.Fail("Failed to retrieve folders. Please try again later.");
        }
    }
}
