using Alcedo.Services;

namespace Alcedo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            SettingsService.RefreshAppTheme();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}