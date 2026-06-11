using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sentinal.Application.Users.Commands;

namespace Sentinal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        return NotFound("Not implemented Register");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        return NotFound("Not implemented Login");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        return NotFound("Not implemented Logout");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        return NotFound("Not implemented GetUser");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id)
    {
        return NotFound("Not implemented UpdateUser");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        return NotFound("Not implemented DeleteUser");
    }
}