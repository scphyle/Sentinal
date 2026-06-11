using Microsoft.AspNetCore.Mvc;

namespace Sentinal.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FolderController : ControllerBase
{

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