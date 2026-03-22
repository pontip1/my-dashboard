using MyDashboard.Models.Entities;
using MyDashboard.Services.Backend;

namespace MyDashboard.Services.Profiles;

public class ProfilesService
{
    public async Task<Profile?> GetCurrentUserProfile()
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
            return null;

        var userId = Guid.Parse(user.Id);

        var response = await SupabaseService.Client
            .From<Profile>()
            .Where(x => x.Id == userId)
            .Get();

        return response.Models.FirstOrDefault();
    }

    public async Task<Profile?> GetProfileById(Guid userId)
    {
        var response = await SupabaseService.Client
            .From<Profile>()
            .Where(x => x.Id == userId)
            .Get();

        return response.Models.FirstOrDefault();
    }
}