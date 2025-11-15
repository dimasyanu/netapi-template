
using Microsoft.AspNetCore.Mvc;
using NetApi.Abstractions;
using NetApi.Models.Dtos;

namespace NetApi.Controllers;

[ApiController]
[Route("Api/Users")]
public class UserController(IUserService service) : ControllerBase
{
    private readonly IUserService _service = service;

    [HttpGet]
    public ActionResult<Result<Paginated<UserDto>>> GetUsers([FromQuery] UserQueryParameters queryParameters)
    {
        var users = _service.GetUsersAsync(queryParameters);
        return Ok(new Result<Paginated<UserDto>>
        {
            Success = true,
            Data = users.Result
        });
    }

    [HttpGet("{id}")]
    public ActionResult<Result<UserDto>> GetUserById(int id)
    {
        var user = _service.GetUserByIdAsync(id);
        return Ok(new Result<UserDto>
        {
            Success = true,
            Data = user.Result
        });
    }

    [HttpPost]
    public ActionResult<Result<CreationDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var user = _service.CreateUserAsync(createUserDto);
        return Created($"Api/Users/{user.Result.Id}", new Result<CreationDto>
            {
                Success = true,
                Data = new CreationDto { Id = user.Result.Id }
            });
    }

    [HttpPatch("{id}")]
    public ActionResult<Result<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        var user = _service.UpdateUserAsync(id, updateUserDto);
        return Ok(new Result<UserDto>
        {
            Success = true,
            Data = user.Result
        });
    }

    [HttpDelete("{id}")]
    public ActionResult<Result<bool>> DeleteUser(int id)
    {
        _service.DeleteUserAsync(id);
        return NoContent();
    }
}
