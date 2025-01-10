using Alcedo.Services.SettingsService;
using Alcedo.ViewModels;

namespace Alcedo.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(ISettingsService settingsService)
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel(settingsService);
    }
}
