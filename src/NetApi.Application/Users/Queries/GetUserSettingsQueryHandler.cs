using NetApi.Application.Common.Contracts;
using NetApi.Domain.Users;

namespace NetApi.Application.Users.Queries;

public class GetUserSettingsQueryHandler(IUserSettingRepository userSettingRepository) : IQueryHandler<GetUserSettingsQuery, UserSetting?>
{
    private readonly IUserSettingRepository _userSettingRepository = userSettingRepository;

    public async Task<UserSetting?> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _userSettingRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return UserSetting.FromEntities(settings);
    }
}
