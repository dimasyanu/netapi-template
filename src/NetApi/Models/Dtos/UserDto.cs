using NetApi.Application.Common.Exceptions;
using NetApi.Domain.Users;

namespace NetApi.Models.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }

    private UserDto(
        Guid id,
        string firstName,
        string lastName,
        string emailAddress
    )
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
    }

    public static UserDto FromDomainModel(User user)
    {
        if (user == null || user.Id == null) throw new NotFoundException("User not found");
        return new(
            user.Id.ToGuid(),
            user.FirstName,
            user.LastName,
            user.EmailAddress.ToString()
        );
    }
}
