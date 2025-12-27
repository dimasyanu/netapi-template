using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetApi.Constants;
using NetApi.Domain.Users;
using NetApi.Models;
using NetApi.Models.Dtos;

namespace NetApi.Controllers;

public class BaseRestApiController : ControllerBase
{
    protected readonly IMediator Mediator;
    protected User? CurrentUser => (User?)(Request.HttpContext.Items[AuthConstant.CURRENT_USER_KEY] ?? null);

    public BaseRestApiController(IMediator mediator)
    {
        Mediator = mediator;
    }

    protected ActionResult<Result<TData>> Success<TData>(TData data, string message = "Success.")
    {
        return Ok(new Result<TData> {
            Success = true,
            Message = message,
            Data = data,
        });
    }

    protected ActionResult<Result<CreationDto<TKey>>> Created<TKey>(TKey id, string message = "Successfully created.")
    {
        return StatusCode((int)HttpStatusCode.Created, new Result<CreationDto<TKey>> {
            Success = true,
            Message = message,
            Data = new CreationDto<TKey>(id),
        });
    }
}
