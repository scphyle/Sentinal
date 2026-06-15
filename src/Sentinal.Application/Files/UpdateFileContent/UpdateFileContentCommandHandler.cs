using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;

namespace Sentinal.Application.Files.UpdateFileContent;

public class UpdateFileContentCommandHandler : IRequestHandler<UpdateFileContentCommand, Result<Guid>>
{
    private ILogger<UpdateFileContentCommandHandler> _logger;
    private IFolderRepository _folderRepository;
    private IFileRepository _fileRepository;
    private IUserRepository _userRepository;
    private IFileStorageService _fileStorageService;

    public UpdateFileContentCommandHandler(IFolderRepository folderRepository, IFileRepository fileRepository, IUserRepository userRepository, IFileStorageService fileStorageService, ILogger<UpdateFileContentCommandHandler> logger)
    {
        _folderRepository = folderRepository;
        _fileRepository = fileRepository;
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }
    
    public async Task<Result<Guid>> Handle(UpdateFileContentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (newFile, _) = await _fileRepository.UpdateFileContentAsync(request.FileId, request.UserId,request.FileSize, request.Description);
            await _fileStorageService.SaveFileAsync(request.UserId, newFile.Id, request.Stream);
            return Result.Ok(newFile.Id);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update file content");
            return Result.Fail("Failed to update file content");
        }
    }
}