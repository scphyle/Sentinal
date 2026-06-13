using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinal.Api.Models.Files;
using Sentinal.Application.FIles.Create;
using Sentinal.Application.Files.Delete;
using Sentinal.Application.FIles.GetAllFiles;
using Sentinal.Application.FIles.GetAllFilesInAFolder;
using Sentinal.Application.FIles.GetFile;
using Sentinal.Application.FIles.GetFilesInRecycleBin;
using Sentinal.Application.Files.Move;
using Sentinal.Application.FIles.SearchFileByName;
using Sentinal.Application.FIles.UpdateFileDescription;
using Sentinal.Application.FIles.UpdateFileName;
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
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var query = new GetFileByIdQuery(fileId, userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpGet("Allfiles")]
    public async Task<IActionResult> GetAllFiles(CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var query = new GetAllFilesQuery(userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpGet("Allfolders")]
    public async Task<IActionResult> GetAllFolders(CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var query = new GetAllFoldersQuery(userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);

    }

    [HttpGet("AllFilesInFolder/{folderId:guid}")]
    public async Task<IActionResult> GetAllFilesInFolder(Guid folderId, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var query = new GetAllFilesInFolderQuery(folderId, userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpGet("AllFilesInRecycleBin")]
    public async Task<IActionResult> GetAllFilesInRecycleBin(CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var query = new GetAllFilesInRecycleBinQuery(userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpGet("SearchFileByName/{searchTerm}")]
    public async Task<IActionResult> SearchFileByName([FromQuery] string searchTerm, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);
        var query = new SearchFileByNameQuery(searchTerm, userId);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> SaveFile([FromBody] CreateFileRequest createFileRequest, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        if (createFileRequest.FolderId == Guid.Empty)
            createFileRequest.FolderId = userId;

        var query = new CreateFileCommand(createFileRequest.FileName,
            createFileRequest.ContentType,
            createFileRequest.File.OpenReadStream(),
            createFileRequest.File.Length,
            userId,
            createFileRequest.FolderId,
            createFileRequest.Description);
        var commandResult = await _mediator.Send(query, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPatch("MoveFile")]
    public async Task<IActionResult> MoveFile([FromBody] MoveFileRequest moveFileRequest, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

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
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

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
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);
        var command = new UpdateFileNameCommand(updateFileDataRequest.FileId, userId, updateFileDataRequest.NewName);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFile([FromBody] UpdateFileDataRequest updateFileDataRequest, 
        CancellationToken ct)
    {
        //TODO: Implment the Update of the file content (maybe should implment file history?)
        return BadRequest("Not implemented");
    }

[HttpDelete("{fileId:guid}")]
    public async Task<IActionResult> Delete(Guid fileId, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);
        
        var command = new DeleteFileCommand(fileId, userId);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }
}