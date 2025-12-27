using System.ComponentModel.DataAnnotations;
using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;

namespace NetApi.Application.Users.Queries;

public class GetUserByEmailAddressQuery : IQuery<User>
{
    [Required]
    public string EmailAddress { get; init; } = "";

    public GetUserByEmailAddressQuery() { }
    public GetUserByEmailAddressQuery(string emailAddress) => EmailAddress = emailAddress;
}
