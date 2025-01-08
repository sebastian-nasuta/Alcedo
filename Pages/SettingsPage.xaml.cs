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

namespace Alcedo.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        SetCurrentTheme();
    }

    private void SetCurrentTheme()
    {
        var currentTheme = SettingsService.LoadUserAppTheme();

        switch (currentTheme)
        {
            case AppTheme.Dark:
                darkRadioButton.IsChecked = true;
                break;
            case AppTheme.Light:
                lightRadioButton.IsChecked = true;
                break;
            default:
                unspecifiedRadioButton.IsChecked = true;
                break;
        }
    }

    private void OnThemeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is RadioButton radioButton)
        {
            SettingsService.SetAppTheme(radioButton.Value.ToString());
        }
    }
}
