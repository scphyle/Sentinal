using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Files.DTOs;

namespace Sentinal.Application.FIles.GetAllFilesInAFolder;

public class GetAllFilesInFolderQueryHandler : IRequestHandler<GetAllFilesInFolderQuery, Result<List<FileDataDto>>>
{
    private readonly IFileRepository _fileRepository;
    private ILogger<GetAllFilesInFolderQueryHandler> _logger;

    public GetAllFilesInFolderQueryHandler(ILogger<GetAllFilesInFolderQueryHandler> logger, IFileRepository fileRepository)
    {
        _logger = logger;
        _fileRepository = fileRepository;
    }

    public async Task<Result<List<FileDataDto>>> Handle(GetAllFilesInFolderQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
            return Result.Fail("User Id Required");
        if (request.FolderId == Guid.Empty)
            return Result.Fail("Folder Id Required");
        try
        {
            var filesInFolder = await _fileRepository.GetFilesByFolderIdAsync(request.FolderId, request.UserId);
            if (filesInFolder.Count == 0)
                return  Result.Fail<List<FileDataDto>>("No Files Found");

            return Result.Ok(filesInFolder.Select(f => new FileDataDto(f.Id,
                f.FileName,
                f.ContentType,
                f.Description,
                f.CreatedAt,
                f.UpdatedAt,
                f.FolderId)).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all files for {request.UserId}", ex);
            return Result.Fail("Error getting all files");
        }
    }
}