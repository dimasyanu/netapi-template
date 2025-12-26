using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetApi.Application.Common.Exceptions;
using NetApi.Domain.Users;
using NetApi.Models;
using NetApi.Models.Dtos;

namespace NetApi.Controllers;

public class BaseRestApiController : ControllerBase
{
    protected readonly IMediator Mediator;
    protected User? CurrentUser;

    public BaseRestApiController(IMediator mediator)
    {
        Mediator = mediator;
        CurrentUser = Request.HttpContext.Items["CurrentUser"] as User;
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
