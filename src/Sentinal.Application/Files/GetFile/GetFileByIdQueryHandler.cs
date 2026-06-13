using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens.Experimental;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Files.DTOs;

namespace Sentinal.Application.FIles.GetFile;

public class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQuery, Result<FileContentDto>>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    
    private Logger<GetFileByIdQueryHandler> _logger;
    public GetFileByIdQueryHandler(Logger<GetFileByIdQueryHandler> logger, IFileRepository fileRepository, IFileStorageService fileStorageService)
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
            var fileStream = await _fileStorageService.GetFileAsync(request.FileId, request.UserId);
            if (fileStream.Length == 0)
                return Result.Fail("File Not Found");
            if (fileData == null)
                return Result.Fail("File Not Found");

            return Result.Ok(new FileContentDto(fileData.FileName, fileData.ContentType, fileData.Id, fileStream));

        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all files for {request.UserId}", ex);
            return Result.Fail("Error getting all files");
        }
    }
}