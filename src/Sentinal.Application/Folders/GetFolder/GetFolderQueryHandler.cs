using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.GetFolder;

public class GetFolderQueryHandler : IRequestHandler<GetFolderQuery, Result<FolderDto>>
{
    private readonly ILogger<GetFolderQueryHandler> _logger;
    private readonly IFolderRepository _folderRepository;

    public GetFolderQueryHandler(ILogger<GetFolderQueryHandler> logger, IFolderRepository folderRepository)
    {
        _logger = logger;
        _folderRepository = folderRepository;
    }

    public async Task<Result<FolderDto>> Handle(GetFolderQuery query, CancellationToken cancellationToken)
    {
        if (query.Id == Guid.Empty)
            return Result.Fail("Folder id cannot be empty");
        if (query.UserId == Guid.Empty)
            return Result.Fail("User id cannot be empty");

        try
        {
            var folder = await _folderRepository.GetFolderAsync(query.Id, query.UserId);
            if (folder == null)
            {
                _logger.LogWarning("Folder not found. Folder Id: {FolderId}, User Id: {UserId}", query.Id, query.UserId);
                return Result.Fail("Folder not found or does not belong to you");
            }

            _logger.LogInformation("Folder retrieved successfully: {FolderId}", query.Id);
            return Result.Ok(new FolderDto(folder.Id,
                folder.FolderName,
                folder.ParentFolderId,
                folder.CreatedAt,
                folder.UpdatedAt,
                folder.Children,
                folder.Files));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving folder. Folder Id: {FolderId}", query.Id);
            return Result.Fail("Failed to retrieve folder. Please try again later.");
        }
    }
}