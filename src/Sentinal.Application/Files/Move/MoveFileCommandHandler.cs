using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Files.Move;

namespace Sentinal.Application.Files.Move;

public class MoveFileCommandHandler : IRequestHandler<MoveFileCommand, Result<bool> >
{
    private readonly ILogger<MoveFileCommandHandler> _logger;
    private readonly IFileRepository _fileRepository;

    public MoveFileCommandHandler(ILogger<MoveFileCommandHandler> logger, IFileRepository fileRepository)
    {
        _logger = logger;
        _fileRepository = fileRepository;
    }

    public async Task<Result<bool>> Handle(MoveFileCommand request, CancellationToken cancellationToken)
    {
        if (request.FileId == Guid.Empty)
            return Result.Fail("File ID cannot be empty");
        if (request.UserId == Guid.Empty)
            return Result.Fail("User ID cannot be empty");
        if (request.DestinationFolderId == Guid.Empty)
            return Result.Fail("Destination folder cannot be empty");

        try
        {
            _logger.LogInformation("Moving file {RequestDestinationFolderId} to {RequestFileId}", request.DestinationFolderId, request.FileId);
            var success =  await _fileRepository.MoveFileAsync(request.FileId, request.DestinationFolderId,request.UserId);
            if(!success)
                return Result.Fail("Failed to move file");
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving file: {file}", request.FileId);
            return Result.Fail("Failed to move file");
        }
    }
}