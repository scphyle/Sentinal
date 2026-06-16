using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Files.DTOs;
using Sentinal.Domain.Files;

namespace Sentinal.Application.Files.GetAllFiles;


public class GetAllFilesQueryHandler : IRequestHandler<GetAllFilesQuery, Result<List<FileDataDto>>>
{
    private readonly IFileRepository _fileRepository;
    private ILogger<GetAllFilesQueryHandler> _logger;
    public GetAllFilesQueryHandler(IFileRepository fileRepository, ILogger<GetAllFilesQueryHandler> logger)
    {
        _fileRepository = fileRepository;
        _logger = logger;
    }

    public async Task<Result<List<FileDataDto>>> Handle(GetAllFilesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
            return Result.Fail("User Id cannont be empty");
        try
        {
            _logger.LogInformation($"Getting all files for {request.UserId}");
            var allFiles = await _fileRepository.GetAllUserFilesAsync(request.UserId);
            if (allFiles.Count == 0)
                return Result.Fail("No Files found");
            
            var fileDtos = allFiles.Select(f => new FileDataDto(f.Id, 
                f.FileName,f.ContentType ,f.Description, f.CreatedAt, f.UpdatedAt, f.FolderId)).ToList();
            return Result.Ok(fileDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all files for {UserId}", request.UserId);
            return Result.Fail("Failed to get all files");
        }
    }
}