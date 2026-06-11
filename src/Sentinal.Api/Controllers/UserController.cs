using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sentinal.Api.Models.Requests;
using Sentinal.Application.Users.Login;
using Sentinal.Application.Users.Register;

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
    public async Task<ActionResult<RegisterUserDto>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var command = new RegisterUserCommand(request.Username, request.Email, request.Password);
        var commandResult = await _mediator.Send(command, ct);
        if(commandResult.IsSuccess)
        {
            return Ok(new RegisterUserDto(commandResult.Value.Id, commandResult.Value.Username, commandResult.Value.Email));
        }
        return BadRequest(commandResult.Errors);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginUserDto>> Login([FromBody] LoginUserRequest request, CancellationToken ct)
    {
        var command = new LoginUserCommand(request.Password, request.Username, request.Email);
        var commandResult = await _mediator.Send(command, ct);
        if(commandResult.IsSuccess)
        {
            return Ok(commandResult.Value);
        }
        return BadRequest(commandResult.Errors);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        //TODO: Phase 2
        return NotFound("Not implemented Logout");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        //TODO: Phase 2 (displaying in a users profile)
        return NotFound("Not implemented GetUser");
    }

    [HttpPost("update-password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request, CancellationToken ct)
    {
        return NotFound("Not implemented UpdatePassword");
    }
    [HttpPost("update-email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateUserEmailRequest request, CancellationToken ct)
    {
        return NotFound("Not implemented UpdateEmail");
    }

    [HttpPost("update-username")]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request, CancellationToken ct)
    {
        return NotFound("Not implemented UpdateUsername");
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] UpdateUserEmailConfirmedRequest request, CancellationToken ct)
    {
        return NotFound("Not implemented ConfirmEmail");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        return NotFound("Not implemented DeleteUser");
    }
}