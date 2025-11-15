using Microsoft.AspNetCore.Mvc;
using NetApi.Models.Dtos;

namespace NetApi.Abstractions;

public interface IUserService
{
    Task<Paginated<UserDto>> GetUsersAsync([FromQuery] UserQueryParameters queryParameters);
    Task<UserDto> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(int id);
}
