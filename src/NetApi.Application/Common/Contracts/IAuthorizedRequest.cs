using NetApi.Domain.Users;

namespace NetApi.Application.Common.Contracts;

public interface IAuthorizedRequest
{
    User? User { get; init; }
    bool IsAuthenticated();
    (string, byte) GetRequestPermission();
}

