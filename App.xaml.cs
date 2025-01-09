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

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}