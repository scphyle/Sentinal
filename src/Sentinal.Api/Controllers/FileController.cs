using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinal.Api.Extensions;
using Sentinal.Api.Models.Files;
using Sentinal.Application.Files.Create;
using Sentinal.Application.Files.Delete;
using Sentinal.Application.Files.GetAllFiles;
using Sentinal.Application.Files.GetAllFilesInAFolder;
using Sentinal.Application.Files.GetFile;
using Sentinal.Application.Files.GetFilesInRecycleBin;
using Sentinal.Application.Files.Move;
using Sentinal.Application.Files.SearchFileByName;
using Sentinal.Application.Files.UpdateFileContent;
using Sentinal.Application.Files.UpdateFileDescription;
using Sentinal.Application.Files.UpdateFileName;
using Sentinal.Application.Folders.GetAllFolders;

namespace Sentinal.Api.Controllers;


[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FileController : ControllerBase
{

    private readonly IMediator _mediator;

    public FileController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpGet("{fileId:guid}")]
    public async Task<IActionResult> GetById(Guid fileId, CancellationToken ct)
    {
        var userId = User.GetUserId();

        var query = new GetFileByIdQuery(fileId, userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsFailed)
            return BadRequest(commandResult.Errors);

        var fileContent = commandResult.Value;
        return File(
            fileContent.FileStream,
            fileContent.ContentType,
            fileContent.FileName
        );
    }

    [HttpGet("Allfiles")]
    public async Task<IActionResult> GetAllFiles(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var query = new GetAllFilesQuery(userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpGet("Allfolders")]
    public async Task<IActionResult> GetAllFolders(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var query = new GetAllFoldersQuery(userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);

    }

    [HttpGet("AllFilesInFolder/{folderId:guid}")]
    public async Task<IActionResult> GetAllFilesInFolder(Guid folderId, CancellationToken ct)
    {
        var userId = User.GetUserId();

        var query = new GetAllFilesInFolderQuery(folderId, userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpGet("AllFilesInRecycleBin")]
    public async Task<IActionResult> GetAllFilesInRecycleBin(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var query = new GetAllFilesInRecycleBinQuery(userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpGet("SearchFileByName")]
    public async Task<IActionResult> SearchFileByName([FromQuery] string searchTerm, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var query = new SearchFileByNameQuery(searchTerm, userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> SaveFile([FromForm] SaveFileRequest saveFileRequest, CancellationToken ct)
    {
        var userId = User.GetUserId();

        if (saveFileRequest.FolderId == Guid.Empty)
            saveFileRequest.FolderId = userId;

        var query = new CreateFileCommand(saveFileRequest.FileName,
            saveFileRequest.ContentType,
            saveFileRequest.File.OpenReadStream(),
            saveFileRequest.File.Length,
            userId,
            saveFileRequest.FolderId,
            saveFileRequest.Description);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPatch("MoveFile")]
    public async Task<IActionResult> MoveFile([FromBody] MoveFileRequest moveFileRequest, CancellationToken ct)
    {
        var userId = User.GetUserId();

        var command = new MoveFileCommand(moveFileRequest.FileId,
            moveFileRequest.DestinationFolderId,
            userId);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPatch("UpdateFileDescription")]
    public async Task<IActionResult> UpdateFileDescription([FromBody] UpdateFileDataRequest updateFileDataRequest,
        CancellationToken ct)
    {
        if (updateFileDataRequest.FileId == Guid.Empty)
            return BadRequest("FileId is required");
        if (updateFileDataRequest.NewDescription == string.Empty)
            return BadRequest("NewDescription is required");
        var userId = User.GetUserId();

        var command = new UpdateFileDescriptionCommand(updateFileDataRequest.FileId, 
                userId, updateFileDataRequest.NewDescription!);
        
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPatch("UpdateFileName")]
    public async Task<IActionResult> UpdateFileName([FromBody] UpdateFileDataRequest updateFileDataRequest, 
        CancellationToken ct)
    {
        if(updateFileDataRequest.FileId == Guid.Empty)
            return BadRequest("FileId is required");
        if (string.IsNullOrEmpty(updateFileDataRequest.NewName))
            return BadRequest("NewName is required");
        var userId = User.GetUserId();
        var command = new UpdateFileNameCommand(updateFileDataRequest.FileId, userId, updateFileDataRequest.NewName);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFile([FromForm] SaveFileRequest updateFileDataRequest,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        var command = new UpdateFileContentCommand(
            updateFileDataRequest.FileId ?? throw new ArgumentNullException(nameof(updateFileDataRequest.FileId)),
            updateFileDataRequest.File.OpenReadStream(),
            updateFileDataRequest.File.Length,
            userId,
            updateFileDataRequest.Description);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
        
    }

[HttpDelete("{fileId:guid}")]
    public async Task<IActionResult> Delete(Guid fileId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        var command = new DeleteFileCommand(fileId, userId);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }
}