using NetApi.Application.Common.Contracts;
using NetApi.Application.Common.Exceptions;

namespace NetApi.Application.Users.Commands;

public class SaveUserSettingsCommandHandler(IUserSettingRepository repo) : ICommandHandler<SaveUserSettingsCommand, bool>
{
    private readonly IUserSettingRepository _repo = repo;

    public async Task<bool> Handle(SaveUserSettingsCommand request, CancellationToken cancellationToken)
    {
        if (request.User == null || (!request.User.Roles!.Any(x => x.Name == "admin") && request.User.Id != request.UserId))
            throw new UnauthorizedException();

        var existingSettings = await _repo.GetByUserIdAsync(request.UserId, cancellationToken);

        var userSettingEntities = request.UserSettings.ToEntities(request.UserId);
        userSettingEntities.ForEach(s => {
            var existing = existingSettings.FirstOrDefault(es => es.Key == s.Key);
            if (existing != null) {
                s.CreatedAt = DateTime.Now;
                s.CreatedBy = request.User.Username;
            }
            s.UpdatedAt = DateTime.Now;
            s.UpdatedBy = request.User.Username;
        });
        await _repo.SaveUserSettingsAsync(request.UserId, userSettingEntities, cancellationToken);
        return true;
    }
}
