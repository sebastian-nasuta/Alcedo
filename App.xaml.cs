/* Unmerged change from project 'Alcedo (net9.0-windows10.0.19041.0)'
Added:
using Alcedo.Services.SettingsService;
using Alcedo.Services.SettingsService.SettingsService;
using Alcedo.Services.SettingsService.SettingsService.SettingsService;
*/

/* Unmerged change from project 'Alcedo (net9.0-maccatalyst)'
Added:
using Alcedo.Services.SettingsService;
using Alcedo.Services.SettingsService.SettingsService;
*/

/* Unmerged change from project 'Alcedo (net9.0-ios)'
Added:
using Alcedo.Services.SettingsService;
*/
using Alcedo.Services.SettingsService;

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