using CommunityToolkit.Maui.Views;
using MyDashboard.Services.Achievements;

namespace MyDashboard.Pages.Popups;

public partial class AchievementsPopup : Popup
{
    private readonly AchievementsService _achievementsService = new();
    public AchievementsPopup()
    {
        InitializeComponent();
        _ = LoadAchievements();
    }

    private async Task LoadAchievements()
    {
        try
        {
            var items = await _achievementsService.GetAchievementsWithStatus();

            AchievementsList.Children.Clear();

            if (items.Count == 0)
            {
                AchievementsList.Children.Add(new Label
                {
                    Text = "No achievements"
                });
                return;
            }

            foreach (var item in items)
            {
                bool unlocked = item.IsUnlocked;

                var frame = new Frame
                {
                    Padding = 12,
                    Margin = new Thickness(0, 0, 0, 8),
                    BorderColor = unlocked ? Colors.Green : Colors.Red
                };

                var layout = new VerticalStackLayout
                {
                    Spacing = 5
                };

                layout.Children.Add(new Label
                {
                    Text = item.Title,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 18,
                    TextColor = unlocked ? Colors.Green : Colors.Red
                });

                layout.Children.Add(new Label
                {
                    Text = item.Description,
                    TextColor = Colors.Black
                });

                layout.Children.Add(new Label
                {
                    Text = unlocked ? "Completed" : "Not completed",
                    FontAttributes = FontAttributes.Bold,
                    TextColor = unlocked ? Colors.Green : Colors.Red
                });

                frame.Content = layout;
                AchievementsList.Children.Add(frame);
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