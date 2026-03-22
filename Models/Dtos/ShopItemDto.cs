using System.Text.Json.Serialization;

namespace MyDashboard.Models.Dtos;

public class ShopItemDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("is_one_time")]
    public bool IsOneTime { get; set; }

    [JsonPropertyName("is_purchased")]
    public bool IsPurchased { get; set; }
}