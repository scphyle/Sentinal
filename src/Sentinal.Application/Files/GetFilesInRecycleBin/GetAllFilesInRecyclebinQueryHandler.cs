using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Files.DTOs;

namespace Sentinal.Application.Files.GetFilesInRecycleBin;

public class GetAllFilesInRecyclebinQueryHandler : IRequestHandler<GetAllFilesInRecycleBinQuery, Result<List<FileDataDto>>>
{
    private readonly IFileRepository _fileRepository;
    private ILogger<GetAllFilesInRecyclebinQueryHandler> _logger;
    public GetAllFilesInRecyclebinQueryHandler(IFileRepository fileRepository, ILogger<GetAllFilesInRecyclebinQueryHandler> logger)
    {
        _fileRepository = fileRepository;
        _logger = logger;
    }
    public async Task<Result<List<FileDataDto>>> Handle(GetAllFilesInRecycleBinQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
            return Result.Fail("User Id Required");

        try
        {
            var deletedFiles = await _fileRepository.GetFilesMarkedForDeletionAsync(request.UserId);
            return Result.Ok(deletedFiles.Select(f =>
                new FileDataDto(f.Id, f.FileName, f.ContentType, f.Description, f.CreatedAt, f.UpdatedAt, f.FolderId)).ToList());

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files marked for deletion");
            return Result.Fail("Error getting files marked for deletion");
        }
    }
}