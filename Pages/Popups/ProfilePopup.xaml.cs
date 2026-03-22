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
            AvatarImage.Source = null;
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
        {
            NameField.Text = profile.Name ?? "";

            if (!string.IsNullOrWhiteSpace(profile.AvatarUrl))
                AvatarImage.Source = ImageSource.FromUri(new Uri(profile.AvatarUrl));
            else
                AvatarImage.Source = null;
        }
        else
        {
            NameField.Text = "";
            AvatarImage.Source = null;
        }
    }

    private async void ChangePhoto_Clicked(object sender, EventArgs e)
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "User not found", "OK");
            return;
        }

        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo == null)
                return;

            var userId = Guid.Parse(user.Id);
            var userIdText = user.Id;

            var extension = Path.GetExtension(photo.FileName);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".jpg";

            var filePath = $"{userIdText}/avatar{extension}";

            await using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            await SupabaseService.Client.Storage
                .From("avatars")
                .Upload(bytes, filePath, new Supabase.Storage.FileOptions
                {
                    Upsert = true
                });

            var publicUrl = SupabaseService.Client.Storage
                .From("avatars")
                .GetPublicUrl(filePath);

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

            profile.AvatarUrl = publicUrl;
            await profile.Update<Profile>();

            AvatarImage.Source = ImageSource.FromUri(new Uri(publicUrl));

            await Application.Current.MainPage.DisplayAlert("Success", "Photo updated", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
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

            var updatedProfile = new Profile
            {
                Id = profile.Id,
                Email = profile.Email,
                Name = newName,
                Points = profile.Points,
                ShopPoints = profile.ShopPoints,
                AvatarUrl = profile.AvatarUrl
            };

            await updatedProfile.Update<Profile>();

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