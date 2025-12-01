using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Application.Users.Queries;


public record GetUserSettingsQuery(UserId UserId) : IQuery<UserSetting?>;