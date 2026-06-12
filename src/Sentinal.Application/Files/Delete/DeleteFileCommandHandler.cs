using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;

namespace Sentinal.Application.Files.Delete;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result<bool>>
{
    private readonly IFileRepository _fileRepository;
    private readonly ILogger<DeleteFileCommandHandler> _logger;

    public DeleteFileCommandHandler(IFileRepository fileRepository, ILogger<DeleteFileCommandHandler> logger)
    {
        _fileRepository = fileRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        if (request.FileId == Guid.Empty)
            return Result.Fail("FileId cannot be empty");
        if (request.UserId == Guid.Empty)
            return Result.Fail("UserId cannot be empty");

        try
        {
            var result = await _fileRepository.MarkFileAsDeletedAsync(request.FileId, request.UserId);
            return result ? Result.Ok(true) : Result.Fail("File not found or user does not own this file");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return Result.Fail("Error deleting file");
        }
    }
}