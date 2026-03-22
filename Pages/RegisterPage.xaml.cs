using MyDashboard.Services.Backend;


namespace MyDashboard.Pages;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
	}

    private async void Register_Clicked(object sender, EventArgs e)
    {
        string name = Name.Text;
        string email = Email.Text;
        string password = Password.Text;
        string confirmPassword = PasswordConfirm.Text;

        bool isValid = await CheckUsersData(name, email, password, confirmPassword);

        if (!isValid)
            return;

        try
        {
            var options = new Supabase.Gotrue.SignUpOptions
            {
                Data = new Dictionary<string, object>
                {
                    { "name", name }
                }
            };

            var response = await SupabaseService.Client.Auth.SignUp(email, password, options);

            await DisplayAlert("Success", "Account created successfully", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }

    private async Task<bool> CheckUsersData(string name, string email, string password, string confirmPassword)
    {
        if (name.Length > 8)
        {
            await DisplayAlert("Error", "Name is too long", "Ok");
            return false;
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Error", "Name is empty", "OK");
            return false;
        }
        if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Passwords do not match", "Ok");
            return false;
        }
        if (password.Length < 5)
        {
            await DisplayAlert("Error", "Passwords is too short", "Ok");
            return false;
        }
        if (!email.Contains('@'))
        {
            await DisplayAlert("Error", "Your email is not correct", "Ok");
            return false;
        }
        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Error", "Email is empty", "Ok");
            return false;
        }
        return true;
    }


}