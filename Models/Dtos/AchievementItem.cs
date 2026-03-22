using System.Text.Json.Serialization;

namespace MyDashboard.Models.Dtos;

public class AchievementItem
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("is_unlocked")]
    public bool IsUnlocked { get; set; }
}