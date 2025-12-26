using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetApi.Application.Auth.Commands;
using NetApi.Application.Common.Exceptions;
using NetApi.Domain.Auth.Models;
using NetApi.Models;
using NetApi.Models.Dtos;

namespace NetApi.Controllers;

[ApiController]
[Route("v1/Auth")]
public class AuthController(IMediator mediator) : BaseRestApiController(mediator)
{
    [HttpPost("Login")]
    public async Task<ActionResult<Result<LoginResult>>> Login([FromBody] LoginCommand command)
    {
        // Placeholder implementation
        var result = await Mediator.Send(command);
        return Ok(new Result<LoginResult> {
            Success = true,
            Data = result,
            Message = "Success"
        });
    }

    [Authorize]
    [HttpGet("Check")]
    public ActionResult<Result<UserDto>> Check()
    {
        if (CurrentUser == null) throw new UnauthorizedException();
        return Success(UserDto.FromDomainModel(CurrentUser), "User is authenticated");
    }
}
