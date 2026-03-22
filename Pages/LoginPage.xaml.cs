using MyDashboard.Services.Backend;

namespace MyDashboard.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void Login_Clicked(object sender, EventArgs e)
    {
        LoginButton.IsEnabled = false;

        string email = Email.Text;
        string password = Password.Text;

        if (!await CheckLoginData(email, password))
        {
            LoginButton.IsEnabled = true;
            return;
        }

        try
        {
            var session = await SupabaseService.Client.Auth.SignIn(email, password);

            await DisplayAlert("Success", "You logged in successfully", "OK");
            await Shell.Current.GoToAsync("//DashBoardPage");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            await DisplayAlert("Login error", ex.Message, "OK");
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private async Task<bool> CheckLoginData(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Error", "Email is empty", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Password is empty", "OK");
            return false;
        }

        if (!email.Contains('@'))
        {
            await DisplayAlert("Error", "Your email is not correct", "OK");
            return false;
        }

        return true;
    }
}