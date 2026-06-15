using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentinal.Api.Models.Requests;
using Sentinal.Application.Users.ConfirmEmail;
using Sentinal.Application.Users.Delete;
using Sentinal.Application.Users.DTOs;
using Sentinal.Application.Users.Login;
using Sentinal.Application.Users.Register;
using Sentinal.Application.Users.UpdateEmail;
using Sentinal.Application.Users.UpdatePassword;
using Sentinal.Application.Users.UpdateUsername;

namespace Sentinal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserAuthDto>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var command = new RegisterUserCommand(request.Username, request.Email, request.Password);
        var commandResult = await _mediator.Send(command, ct);
        if(commandResult.IsSuccess)
        {
            return Ok(commandResult.Value);
        }
        return BadRequest(commandResult.Errors);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<UserAuthDto>> Login([FromBody] LoginUserRequest request, CancellationToken ct)
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
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var command = new UpdatePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPost("update-email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateUserEmailRequest request, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var command = new UpdateUserEmailCommand(userId, request.NewEmail);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPost("update-username")]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var command = new UpdateUsernameCommand(userId, request.NewUsername);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);

        var command = new ConfirmEmailCommand(userId);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        Guid.TryParse(User.FindFirstValue("userId"), out Guid userId);
        if (id != userId)
            return Forbid();

        var command = new DeleteUserCommand(userId);
        var commandResult = await _mediator.Send(command, ct);
        if (commandResult.IsSuccess)
            return Ok(commandResult.Value);
        return BadRequest(commandResult.Errors);
    }
}
