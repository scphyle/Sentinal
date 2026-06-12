using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.SearchFolderByName;

public class SearchFolderByNameQueryHandler : IRequestHandler<SearchFolderByNameQuery, Result<List<FolderDto>>>
{
    private readonly ILogger<SearchFolderByNameQueryHandler> _logger;
    private readonly IFolderRepository _folderRepository;

    public SearchFolderByNameQueryHandler(ILogger<SearchFolderByNameQueryHandler> logger, IFolderRepository folderRepository)
    {
        _logger = logger;
        _folderRepository = folderRepository;
    }

    public async Task<Result<List<FolderDto>>> Handle(SearchFolderByNameQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Name))
            return Result.Fail("Folder name cannot be empty");
        if (query.UserId == Guid.Empty)
            return Result.Fail("User id cannot be empty");

        try
        {
            var folders = await _folderRepository.GetFolderByNameAsync(query.Name, query.UserId);
            if (folders == null || folders.Count == 0)
            {
                _logger.LogInformation("No folders found with name: {FolderName} for user: {UserId}", query.Name, query.UserId);
                return Result.Ok(new List<FolderDto>());
            }

            var folderDtos = folders.Select(f => new FolderDto(f.Id, f.FolderName, f.ParentFolderId, f.CreatedAt, f.UpdatedAt)).ToList();
            _logger.LogInformation("Found {Count} folders with name: {FolderName} for user: {UserId}", folderDtos.Count, query.Name, query.UserId);
            return Result.Ok(folderDtos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error searching folders by name. Folder Name: {FolderName}, User Id: {UserId}", query.Name, query.UserId);
            return Result.Fail("Failed to search folders. Please try again later.");
        }
    }
}
