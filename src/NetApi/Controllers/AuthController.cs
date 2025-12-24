using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetApi.Application.Auth.Commands;
using NetApi.Domain.Auth.Models;
using NetApi.Models;

namespace NetApi.Controllers;

[ApiController]
[Route("v1/Auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("Login")]
    public async Task<ActionResult<Res<LoginResult>>> Login([FromBody] LoginCommand command)
    {
        // Placeholder implementation
        var result = await _mediator.Send(command);
        return Ok(new Res<LoginResult> {
            Success = true,
            Data = result,
            Message = "Success"
        });
    }

    [Authorize]
    [HttpGet("Check")]
    public ActionResult<Res<User>> Check()
    {
        return Ok(new Res<User> { Data = new User { Id = 1, Name = "Test User" } });
    }
}
