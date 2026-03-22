using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using MyDashboard.Services.Leaderboard;
using MyDashboard.Services.Profiles;

namespace MyDashboard.Pages.Popups;

public partial class LeaderboardPopup : Popup
{
    private readonly ProfilesService _profilesService = new();
    private readonly LeaderboardService _leaderboardService = new();
    public LeaderboardPopup()
    {
        InitializeComponent();
        _ = LoadData();
    }

    private async Task LoadData()
    {
        await LoadLeaderboard();
        await LoadMyPoints();
    }

    private async Task LoadMyPoints()
    {
        var profile = await _profilesService.GetCurrentUserProfile();
        MyPointsLabel.Text = $"Your points: {profile?.Points ?? 0}";
    }

    private async Task LoadLeaderboard()
    {
        try
        {
            var items = await _leaderboardService.GetLeaderboard();

            LeaderboardList.Children.Clear();

            int place = 1;

            foreach (var item in items)
            {
                string displayName = !string.IsNullOrWhiteSpace(item.Name)
                    ? item.Name!
                    : item.Email ?? "Unknown user";

                var rowGrid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(new GridLength(50)),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    },
                    ColumnSpacing = 10,
                    Padding = new Thickness(12),
                    BackgroundColor = place <= 3
                        ? Color.FromArgb("#FFF7ED")
                        : Colors.White
                };

                var rowBorder = new Border
                {
                    Stroke = place <= 3
                        ? Color.FromArgb("#FDBA74")
                        : Color.FromArgb("#E5E7EB"),
                    StrokeThickness = 1,
                    Margin = new Thickness(0, 0, 0, 8),
                    Content = rowGrid
                };

                rowBorder.StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(14)
                };

                var placeLabel = new Label
                {
                    Text = $"{place}.",
                    FontAttributes = FontAttributes.Bold,
                    TextColor = place <= 3
                        ? Color.FromArgb("#C2410C")
                        : Color.FromArgb("#374151"),
                    VerticalOptions = LayoutOptions.Center
                };
                rowGrid.Add(placeLabel);
                Grid.SetColumn(placeLabel, 0);

                var nameLabel = new Label
                {
                    Text = displayName,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#111827"),
                    VerticalOptions = LayoutOptions.Center
                };
                rowGrid.Add(nameLabel);
                Grid.SetColumn(nameLabel, 1);

                var pointsLabel = new Label
                {
                    Text = item.Points.ToString(),
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#4F46E5"),
                    VerticalOptions = LayoutOptions.Center
                };
                rowGrid.Add(pointsLabel);
                Grid.SetColumn(pointsLabel, 2);

                LeaderboardList.Children.Add(rowBorder);

                place++;
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