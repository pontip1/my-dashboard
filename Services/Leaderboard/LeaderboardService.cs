using MyDashboard.Models.Dtos;
using MyDashboard.Services.Backend;
using System.Text.Json;

namespace MyDashboard.Services.Leaderboard;

public class LeaderboardService
{
    public async Task<List<LeaderboardItem>> GetLeaderboard()
    {
        var result = await SupabaseService.Client
            .Rpc("get_leaderboard", new Dictionary<string, object>());

        var json = result.Content;

        var items = JsonSerializer.Deserialize<List<LeaderboardItem>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return items ?? new List<LeaderboardItem>();
    }
}