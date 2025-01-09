using Alcedo.Services.SettingsService;

namespace Alcedo
{
    public partial class App : Application
    {
        public App(ISettingsService settingsService)
        {
            InitializeComponent();
            settingsService.RefreshAppTheme();
        }

        public static void ShowExceptionAlert(string message, string title = "Error")
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var mainPage = Current?.Windows[0].Page;
                if (mainPage is not null)
                {
                    await mainPage.DisplayAlert(title, message, "OK");
                }
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}