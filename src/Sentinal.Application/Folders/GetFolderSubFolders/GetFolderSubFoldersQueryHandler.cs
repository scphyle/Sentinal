using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.GetFolderSubFolders;

public class GetFolderSubFoldersQueryHandler : IRequestHandler<GetFolderSubFoldersQuery, Result<List<FolderDto>>>
{
    private readonly ILogger<GetFolderSubFoldersQueryHandler> _logger;
    private readonly IFolderRepository _folderRepository;

    public GetFolderSubFoldersQueryHandler(ILogger<GetFolderSubFoldersQueryHandler> logger, IFolderRepository folderRepository)
    {
        _logger = logger;
        _folderRepository = folderRepository;
    }

    public async Task<Result<List<FolderDto>>> Handle(GetFolderSubFoldersQuery query, CancellationToken cancellationToken)
    {
        if (query.FolderId == Guid.Empty)
            return Result.Fail("Folder id cannot be empty");
        if (query.UserId == Guid.Empty)
            return Result.Fail("User id cannot be empty");

        var parentFolder = await _folderRepository.GetFolderAsync(query.FolderId, query.UserId);
        if (parentFolder == null)
            return Result.Fail("Parent folder not found or does not belong to you");

        try
        {
            var subfolders = await _folderRepository.GetFoldersSubFoldersAsync(query.FolderId, query.UserId);
            if (subfolders == null || subfolders.Count == 0)
            {
                _logger.LogInformation("No subfolders found for folder: {FolderId}", query.FolderId);
                return Result.Ok(new List<FolderDto>());
            }

            var folderDtos = subfolders.Select(f => new FolderDto(f.Id, f.FolderName, f.ParentFolderId, f.CreatedAt, f.UpdatedAt)).ToList();
            _logger.LogInformation("Retrieved {Count} subfolders for folder: {FolderId}", folderDtos.Count, query.FolderId);
            return Result.Ok(folderDtos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving subfolders. Folder Id: {FolderId}", query.FolderId);
            return Result.Fail("Failed to retrieve subfolders. Please try again later.");
        }
    }
}