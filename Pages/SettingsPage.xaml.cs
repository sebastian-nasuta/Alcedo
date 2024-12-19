using Alcedo.Services;

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
