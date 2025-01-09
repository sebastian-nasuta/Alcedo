using Alcedo.Services.SettingsService;

namespace Alcedo.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly ISettingsService _settingsService;

    public SettingsPage(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        InitializeComponent();
        SetCurrentTheme();
        LoadApiKey();
    }

    private void SetCurrentTheme()
    {
        var currentTheme = _settingsService.LoadUserAppTheme();

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
            _settingsService.SetAppTheme(radioButton.Value.ToString());
        }
    }

    private void LoadApiKey()
    {
        apiKeyEditor.Text = _settingsService.LoadApiKey();
    }

    private void OnApiKeyChanged(object sender, TextChangedEventArgs e)
    {
        _settingsService.SaveApiKey(e.NewTextValue);
    }
}
