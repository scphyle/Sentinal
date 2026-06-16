using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;

namespace Sentinal.Application.Files.UpdateFileDescription;

public class UpdateFileDescriptionCommandHandler : IRequestHandler<UpdateFileDescriptionCommand, Result<bool>>
{
    private readonly ILogger<UpdateFileDescriptionCommandHandler> _logger;
    private readonly IFileRepository _fileRepository;

    public UpdateFileDescriptionCommandHandler(ILogger<UpdateFileDescriptionCommandHandler> logger, IFileRepository fileRepository)
    {
        _logger = logger;
        _fileRepository = fileRepository;
    }

    public async Task<Result<bool>> Handle(UpdateFileDescriptionCommand request, CancellationToken cancellationToken)
    {
        if (request.FileId == Guid.Empty)
            return Result.Fail("File Id is required");
        if (request.UserId == Guid.Empty)
            return Result.Fail("User Id is required");
        if (string.IsNullOrEmpty(request.NewDescription))
            return Result.Fail("New description is required");
        try
        {
            return Result.Ok(await _fileRepository.UpdateFileDescriptionAsync(request.FileId, request.NewDescription, request.UserId));

        }
        catch (Exception ex)
        { 
            _logger.LogError(ex, "Error updating file description: {newFileDescription}", request.NewDescription);
            return Result.Fail("Failed to update file description");
        }
        
    }
}