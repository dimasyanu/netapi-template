using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetApi.Application.Users.Commands;
using NetApi.Application.Users.Queries;
using NetApi.Models;
using NetApi.Models.Dtos;

namespace NetApi.Controllers;

[ApiController]
[Route("v1/Users")]
public class UserController(IMediator mediator) : BaseRestApiController(mediator)
{
    [HttpGet]
    public async Task<ActionResult<Result<Paginated<UserDto>>>> GetUsers([FromQuery] GetUsersQuery query)
    {
        // Send query
        var queryResult = await Mediator.Send(query);

        // Map Dto
        var result = new Paginated<UserDto> {
            Items = queryResult.Items.Select(x => UserDto.FromDomainModel(x)),
            PageSize = queryResult.PageSize,
            StartIndex = queryResult.StartIndex,
            TotalCount = queryResult.Total
        };

        return Success(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<UserDto>>> GetUserById(Guid id)
    {
        // Create a get query
        var query = new GetUserByIdQuery { UserId = id };

        // Send the query
        var user = await Mediator.Send(query);

        // Map the result into Dto
        var userDto = UserDto.FromDomainModel(user);

        return Success(userDto, "User retrieved successfully.");
    }

    [HttpPost]
    public async Task<ActionResult<Result<CreationDto<Guid>>>> CreateUser([FromBody] CreateUserCommand command)
    {
        var cmdResult = await Mediator.Send(command);
        return Created(cmdResult.ToGuid());
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Result<UserDto>>> UpdateUser(int id, [FromBody] UpdateUserCommand command)
    {
        var user = await Mediator.Send(command);
        var userDto = UserDto.FromDomainModel(user);
        return Success(userDto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result<bool>>> DeleteUser(Guid id)
    {
        var command = new TrashManyUsersCommand { Ids = [id], User = CurrentUser };
        await Mediator.Send(command);
        return NoContent();
    }
}
