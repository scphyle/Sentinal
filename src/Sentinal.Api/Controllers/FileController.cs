using Microsoft.AspNetCore.Mvc;

namespace Sentinal.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return NotFound("Not implemented Files");
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        return NotFound("Not implemented Files");
    }

    [HttpPut]
    public async Task<IActionResult> Put()
    {
        return NotFound("Not implemented Files");
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        return NotFound("Not implemented Files");
    }
}