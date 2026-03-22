using CommunityToolkit.Maui.Views;
using MyDashboard.Models.Dtos;
using MyDashboard.Services.Profiles;

namespace MyDashboard.Pages;

public partial class UserInfoPopup : Popup
{
    private readonly ProfilesService _profilesService = new();

    public UserInfoPopup(RoomMemberInfo member)
    {
        InitializeComponent();

        UserNameLabel.Text = !string.IsNullOrWhiteSpace(member.Name)
            ? member.Name
            : "(no name)";

        UserEmailLabel.Text = member.Email ?? "Unknown user";

        _ = LoadPoints(member.UserId);
    }

    private async Task LoadPoints(Guid userId)
    {
        var profile = await _profilesService.GetProfileById(userId);
        UserPointsLabel.Text = $"{profile?.Points ?? 0}";
    }

    private async void Close_Clicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}