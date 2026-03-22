using System.Text.Json.Serialization;

namespace MyDashboard.Models.Dtos;

public class LeaderboardItem
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("points")]
    public int Points { get; set; }
}