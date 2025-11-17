using System;
using Microsoft.AspNetCore.Mvc;
using NetApi.Abstractions;
using NetApi.Models.Dtos;

namespace NetApi.Services;

public class UserService : IUserService
{
    private readonly List<UserDto> _users =
    [
        new UserDto { Id = 1, Name = "Alice", Email = "alice@example.com" },
        new UserDto { Id = 2, Name = "Bob", Email = "bob@example.com" }
    ];
    
    public async Task<Paginated<UserDto>> GetUsersAsync([FromQuery] UserQueryParameters queryParameters)
    {
        var paginatedUsers = new Paginated<UserDto>
        {
            Items = _users.Skip(queryParameters.StartIndex).Take(queryParameters.PageSize),
            TotalCount = _users.Count,
            PageSize = queryParameters.PageSize,
            StartIndex = queryParameters.StartIndex
        };

        return await Task.FromResult(paginatedUsers);
    }

    public Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id) ?? throw new Exception("User not found");
        return Task.FromResult(user);
    }

    public Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var newUser = new UserDto
        {
            Id = _users.Max(u => u.Id) + 1,
            Name = createUserDto.Name,
            Email = createUserDto.Email
        };
        _users.Add(newUser);
        return Task.FromResult(newUser);
    }

    public Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = _users.FirstOrDefault(u => u.Id == id) ?? throw new Exception("User not found");
        if (updateUserDto.Name != null)
        {
            user.Name = updateUserDto.Name;
        }
        if (updateUserDto.Email != null)
        {
            user.Email = updateUserDto.Email;
        }
        return Task.FromResult(user);
    }

    public Task<bool> DeleteUserAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        _users.Remove(user);
        return Task.FromResult(true);
    }
}
