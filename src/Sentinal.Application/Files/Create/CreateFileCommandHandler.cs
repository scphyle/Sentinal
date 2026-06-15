using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.FIles.Create;

namespace Sentinal.Application.Files.Create;


public class CreateFileCommandHandler : IRequestHandler<CreateFileCommand, Result<Guid>>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<CreateFileCommandHandler> _logger;
    private readonly IFileRepository _fileRepository;
    public CreateFileCommandHandler(IFileStorageService fileStorageService,
        IFileRepository fileRepository,
        ILogger<CreateFileCommandHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _fileRepository = fileRepository;
        _logger = logger;
    }
    public async Task<Result<Guid>> Handle(CreateFileCommand request, CancellationToken cancellationToken)
    {
        if(request.FolderId == Guid.Empty)
            return Result.Fail("FolderId cannot be empty");
        if(request.UserId == Guid.Empty)
            return Result.Fail("UserId cannot be empty");
        if(string.IsNullOrWhiteSpace(request.Name))
            return Result.Fail("Name cannot be empty");
        if(string.IsNullOrWhiteSpace(request.ContentType))
            return Result.Fail("ContentType cannot be empty");

        //TODO: Possible problem with a orphaned record, should be solved using IUnitOfWork
        Domain.Files.FileEntity? file = null;
        try
        {
            file = await _fileRepository.CreateFileAsync(
                request.Name,
                request.FileSize,
                request.ContentType,
                request.UserId,
                request.FolderId,
                request.Description);
            await _fileStorageService.SaveFileAsync(request.UserId, file.Id, request.Stream);
            return Result.Ok(file.Id);
        }
        catch(Exception ex)
        {
            //If something is wrong with the db this will also throw but if something is wrong with the SaveFileAsync
            //then this will clean up the orphaned record
            if (file != null)
                await _fileRepository.MarkFileAsDeletedAsync(file.Id, request.UserId);
            _logger.LogError(ex, "Error creating file");
            return Result.Fail("Error creating file");
        }

    }
}