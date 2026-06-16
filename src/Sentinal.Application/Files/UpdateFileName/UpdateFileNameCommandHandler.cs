using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;

namespace Sentinal.Application.Files.UpdateFileName;

public class UpdateFileNameCommandHandler : IRequestHandler<UpdateFileNameCommand, Result<bool>>
{
    private readonly ILogger<UpdateFileNameCommandHandler> _logger;
    private readonly IFileRepository _fileRepository;

    public UpdateFileNameCommandHandler(ILogger<UpdateFileNameCommandHandler> logger, IFileRepository fileRepository)
    {
        _logger = logger;
        _fileRepository = fileRepository;
    }

    public async Task<Result<bool>> Handle(UpdateFileNameCommand request, CancellationToken cancellationToken)
    {
        if (request.FileId == Guid.Empty)
            return Result.Fail("FileId cannot be empty");
        if (request.UserId == Guid.Empty)
            return Result.Fail("User Id cannot be empty");
        if (string.IsNullOrEmpty(request.NewFileName))
            return Result.Fail("New file name cannot be empty");

        try
        {
            var success = await _fileRepository.UpdateFileNameAsync(request.FileId, request.NewFileName, request.UserId);
            if (!success)
                return Result.Fail("Failed to update file name");
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating file name: {newFileName}", request.NewFileName);
            return Result.Fail("Error updating file name");
        }
        
    }
}