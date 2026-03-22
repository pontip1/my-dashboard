using MyDashboard.Services.Backend;

namespace MyDashboard
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            InitializeSupabase();
        }

        private async void InitializeSupabase()
        {
            await SupabaseService.Initialize();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}