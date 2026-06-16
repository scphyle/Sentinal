using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Files.DTOs;

namespace Sentinal.Application.Files.GetFile;

public class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQuery, Result<FileContentDto>>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;

    private readonly ILogger<GetFileByIdQueryHandler> _logger;
    public GetFileByIdQueryHandler(ILogger<GetFileByIdQueryHandler> logger, IFileRepository fileRepository, IFileStorageService fileStorageService)
    {
        _logger = logger;
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
    }
    public async Task<Result<FileContentDto>> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.FileId == Guid.Empty)
            return Result.Fail("File Id Required");
        if (request.UserId == Guid.Empty)
            return Result.Fail("User Id Required");
        try
        {
            var fileData = await _fileRepository.GetFileAsync(request.FileId, request.UserId);
            if (fileData == null)
                return Result.Fail("File Not Found");

            var fileStream = await _fileStorageService.GetFileAsync(request.UserId, request.FileId);
            if (fileStream.Length == 0)
                return Result.Fail("File Not Found");

            return Result.Ok(new FileContentDto(fileData.FileName, fileData.ContentType, fileData.Id, fileStream));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file {FileId} for {UserId}", request.FileId, request.UserId);
            return Result.Fail("Error getting file");
        }
    }
}