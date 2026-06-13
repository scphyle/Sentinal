using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sentinal.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class FolderController : ControllerBase
{
    private readonly IMediator _mediator;

    public FolderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
       return NotFound("Not implemented Folders");
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        return NotFound("Not implemented Folders");
    }

    [HttpPut]
    public async Task<IActionResult> Put()
    {
       return NotFound("Not implemented Folders");
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
       return NotFound("Not implemented Folders");
    }
}