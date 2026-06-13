using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Files.Move;

namespace Sentinal.Application.FIles.Move;

public class MoveFileCommandHandler : IRequestHandler<MoveFileCommand, Result<bool> >
{
    private ILogger<MoveFileCommandHandler> _logger;
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
            _logger.LogInformation($"Moving file {request.DestinationFolderId} to {request.FileId}");
            return await _fileRepository.MoveFileAsync(request.FileId, request.DestinationFolderId,request.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving file: {file}", request.FileId);
            return Result.Fail("Error Moving File");
        }
    }
}