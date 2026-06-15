using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinal.Api.Models.Folders;
using Sentinal.Application.Folders.Create;
using Sentinal.Application.Folders.Delete;
using Sentinal.Application.Folders.GetAllFolders;
using Sentinal.Application.Folders.GetFolder;
using Sentinal.Application.Folders.GetFolderSubFolders;
using Sentinal.Application.Folders.GetFoldersInRecycleBin;
using Sentinal.Application.Folders.Move;
using Sentinal.Application.Folders.SearchFolderByName;
using Sentinal.Application.Folders.Update;

namespace Sentinal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FolderController : ControllerBase
{
    private readonly IMediator _mediator;

    public FolderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{folderId:guid}")]
    public async Task<IActionResult> GetById(Guid folderId, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var query = new GetFolderQuery(folderId, userId);
        var result = await _mediator.Send(query, ct);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Errors);
    }

    [HttpGet("AllFolders")]
    public async Task<IActionResult> GetAllFolders(CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var query = new GetAllFoldersQuery(userId);
        var result = await _mediator.Send(query, ct);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Errors);
    }

    [HttpGet("Subfolders/{folderId:guid}")]
    public async Task<IActionResult> GetSubfolders(Guid folderId, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var query = new GetFolderSubFoldersQuery(folderId, userId);
        var result = await _mediator.Send(query, ct);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Errors);
    }

    [HttpGet("RecycleBin")]
    public async Task<IActionResult> GetRecycleBin(CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var query = new GetFoldersInRecycleBinQuery(userId);
        var result = await _mediator.Send(query, ct);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Errors);
    }

    [HttpGet("SearchFolderByName/{searchTerm}")]
    public async Task<IActionResult> SearchFolderByName(string searchTerm, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var query = new SearchFolderByNameQuery(searchTerm, userId);
        var result = await _mediator.Send(query, ct);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var command = new CreateFolderCommand(request.Name, userId, request.ParentId);
        var result = await _mediator.Send(command, ct);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Errors);
    }

    [HttpPatch("{folderId:guid}/Name")]
    public async Task<IActionResult> UpdateFolderName(Guid folderId, [FromBody] UpdateFolderNameRequest request, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var command = new UpdateFolderNameCommand(folderId, request.NewName, userId);
        var result = await _mediator.Send(command, ct);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Errors);
    }

    [HttpPatch("{folderId:guid}/Move")]
    public async Task<IActionResult> MoveFolder(Guid folderId, [FromBody] MoveFolderRequest request, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var command = new MoveFolderCommand(folderId, request.DestinationFolderId, userId);
        var result = await _mediator.Send(command, ct);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Errors);
    }

    [HttpDelete("{folderId:guid}")]
    public async Task<IActionResult> DeleteFolder(Guid folderId, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var command = new DeleteFolderCommand(folderId, userId);
        var result = await _mediator.Send(command, ct);
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(result.Errors);
    }
}