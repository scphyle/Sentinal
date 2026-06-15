using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Application.Files.DTOs;

namespace Sentinal.Application.FIles.SearchFileByName;

public class SearchFileByNameQueryHandler : IRequestHandler<SearchFileByNameQuery, Result<List<FileDataDto>>>
{
    private readonly ILogger<SearchFileByNameQueryHandler> _logger;
    private readonly IFileRepository _fileRepository;

    public SearchFileByNameQueryHandler(ILogger<SearchFileByNameQueryHandler> logger, IFileRepository fileRepository)
    {
        _logger = logger;
        _fileRepository = fileRepository;
    }

    public async Task<Result<List<FileDataDto>>> Handle(SearchFileByNameQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.FileName))
            return Result.Fail("File Name Is Required");
        if (request.UserId == Guid.Empty)
            return Result.Fail("User Id Is Required");

        try
        {
            var foundFiles = await _fileRepository.SearchFilesByNameAsync(request.FileName, request.UserId);
            if (foundFiles.Count == 0)
                return Result.Fail("No Files Found");
            return Result.Ok(foundFiles.Select(x => new FileDataDto(x.Id,
                x.FileName,
                x.ContentType,
                x.Description,
                x.CreatedAt,
                x.UpdatedAt,
                x.FolderId)).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find files with search term: {searchTerm}", request.FileName);
            return Result.Fail("Failed to find files");
        }
    }
}