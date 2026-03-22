using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using MyDashboard.Services.Profiles;
using MyDashboard.Services.Shop;

namespace MyDashboard.Pages.Popups;

public partial class StorePopup : Popup
{
    private readonly ShopService _shopService = new();
    private readonly ProfilesService _profilesService = new();
    private readonly Func<Task> _onStoreChanged;

    public StorePopup(Func<Task> onStoreChanged)
    {
        InitializeComponent();
        _onStoreChanged = onStoreChanged;
        _ = LoadData();
    }

    private async Task LoadData()
    {
        await LoadMyShopPoints();
        await LoadShopItems();
        await _onStoreChanged();
    }

    private async Task LoadMyShopPoints()
    {
        var profile = await _profilesService.GetCurrentUserProfile();
        MyShopPointsLabel.Text = $"Your shop points: {profile?.ShopPoints ?? 0}";
    }

    private async Task LoadShopItems()
    {
        try
        {
            var items = await _shopService.GetShopItems();

            ShopItemsList.Children.Clear();

            if (items.Count == 0)
            {
                ShopItemsList.Children.Add(new Label
                {
                    Text = "No items in store",
                    FontSize = 16,
                    TextColor = Color.FromArgb("#6B7280")
                });
                return;
            }

            foreach (var item in items)
            {
                var cardGrid = new Grid
                {
                    ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                    ColumnSpacing = 12,
                    Padding = new Thickness(14)
                };

                var cardBorder = new Border
                {
                    Stroke = Color.FromArgb("#E5E7EB"),
                    StrokeThickness = 1,
                    BackgroundColor = Colors.White,
                    Content = cardGrid
                };

                cardBorder.StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(16)
                };

                var textLayout = new VerticalStackLayout
                {
                    Spacing = 6
                };

                textLayout.Children.Add(new Label
                {
                    Text = item.Title,
                    FontSize = 19,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#111827")
                });

                textLayout.Children.Add(new Label
                {
                    Text = item.Description ?? string.Empty,
                    FontSize = 14,
                    TextColor = Color.FromArgb("#6B7280")
                });

                textLayout.Children.Add(new Label
                {
                    Text = $"Price: {item.Price}",
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#4F46E5")
                });

                cardGrid.Add(textLayout);
                Grid.SetColumn(textLayout, 0);

                bool isSold = item.IsOneTime && item.IsPurchased;

                var buyButton = new Button
                {
                    Text = isSold ? "Sold" : "Buy",
                    HeightRequest = 44,
                    CornerRadius = 12,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = isSold
                        ? Color.FromArgb("#E5E7EB")
                        : Color.FromArgb("#4F46E5"),
                    TextColor = isSold
                        ? Color.FromArgb("#6B7280")
                        : Colors.White,
                    VerticalOptions = LayoutOptions.Center,
                    IsEnabled = !isSold
                };

                buyButton.Clicked += async (s, e) =>
                {
                    bool confirm = await Application.Current.MainPage.DisplayAlert(
                        "Buy item",
                        $"Buy '{item.Title}' for {item.Price} shop points?",
                        "Yes",
                        "No");

                    if (!confirm)
                        return;

                    try
                    {
                        await _shopService.BuyItem(item.Id);

                        await LoadMyShopPoints();
                        await LoadShopItems();

                        await Application.Current.MainPage.DisplayAlert(
                            "Success",
                            "Purchase completed",
                            "OK");
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;

                        if (message.Contains("Not enough shop points", StringComparison.OrdinalIgnoreCase))
                            message = "Not enough shop points";

                        if (message.Contains("Item not found", StringComparison.OrdinalIgnoreCase))
                            message = "Item not found";

                        if (message.Contains("already purchased", StringComparison.OrdinalIgnoreCase))
                            message = "Already sold";

                        await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
                    }
                };

                cardGrid.Add(buyButton);
                Grid.SetColumn(buyButton, 1);

                ShopItemsList.Children.Add(cardBorder);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void Close_Clicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}