using MyDashboard.Models.Dtos;
using MyDashboard.Services.Backend;
using System.Text.Json;

namespace MyDashboard.Services.Achievements;

public class AchievementsService
{
    public async Task<List<AchievementItem>> GetAchievementsWithStatus()
    {
        var result = await SupabaseService.Client
            .Rpc("get_achievements_with_status", new Dictionary<string, object>());

        var json = result.Content;

        var items = JsonSerializer.Deserialize<List<AchievementItem>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return items ?? new List<AchievementItem>();
    }
}