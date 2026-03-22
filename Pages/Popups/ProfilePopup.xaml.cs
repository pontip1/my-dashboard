using CommunityToolkit.Maui.Views;
using MyDashboard.Models.Entities;
using MyDashboard.Services.Backend;

namespace MyDashboard.Pages.Popups;

public partial class ProfilePopup : Popup
{
    public ProfilePopup()
    {
        InitializeComponent();
        _ = LoadUserData();
    }

    private async Task LoadUserData()
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
        {
            EmailField.Text = "User not found";
            NameField.Text = "";
            return;
        }

        EmailField.Text = user.Email;

        var userId = Guid.Parse(user.Id);

        var response = await SupabaseService.Client
            .From<Profile>()
            .Where(x => x.Id == userId)
            .Get();

        var profile = response.Models.FirstOrDefault();

        if (profile != null)
            NameField.Text = profile.Name ?? "";
        else
            NameField.Text = "";
    }

    private async void ApplyButton_Clicked(object sender, EventArgs e)
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "User not found", "OK");
            return;
        }

        string newName = NameField.Text;

        if (string.IsNullOrWhiteSpace(newName))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Name cannot be empty", "OK");
            return;
        }

        var userId = Guid.Parse(user.Id);

        try
        {
            var response = await SupabaseService.Client
                .From<Profile>()
                .Where(x => x.Id == userId)
                .Get();

            var profile = response.Models.FirstOrDefault();

            if (profile == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Profile not found", "OK");
                return;
            }

            profile.Name = newName;

            await profile.Update<Profile>();

            await LoadUserData();

            await Application.Current.MainPage.DisplayAlert("Success", "Name updated", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void Logout_Clicked(object sender, EventArgs e)
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert(
        "Logout",
        "Are you sure you want to logout?",
        "Yes",
        "No");
        if (!confirm)
            return;

        await SupabaseService.Client.Auth.SignOut();

        await Shell.Current.GoToAsync("//MainPage");
    }

    private async void Close_Clicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}