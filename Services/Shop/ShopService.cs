using MyDashboard.Models.Dtos;
using MyDashboard.Services.Backend;
using System.Text.Json;

namespace MyDashboard.Services.Shop;

public class ShopService
{
    public async Task<List<ShopItemDto>> GetShopItems()
    {
        var result = await SupabaseService.Client
            .Rpc("get_shop_items", new Dictionary<string, object>());

        var json = result.Content;

        var items = JsonSerializer.Deserialize<List<ShopItemDto>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return items ?? new List<ShopItemDto>();
    }

    public async Task BuyItem(long itemId)
    {
        await SupabaseService.Client
            .Rpc("buy_shop_item", new Dictionary<string, object>
            {
                { "p_shop_item_id", itemId }
            });
    }

    public async Task<bool> HasPurchasedItem(string title)
    {
        var result = await SupabaseService.Client
            .Rpc("has_purchased_item", new Dictionary<string, object>
            {
            { "p_title", title }
            });

        return result.Content.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}