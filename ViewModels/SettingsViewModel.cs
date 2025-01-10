using Alcedo.Services.SettingsService;

namespace Alcedo.ViewModels;

internal partial class SettingsViewModel(ISettingsService settingsService) : ViewModelBase
{
    public string ApiKey
    {
        get => settingsService.LoadApiKey();
        set
        {
            settingsService.SaveApiKey(value);
            OnPropertyChanged();
        }
    }

    public bool IsDarkThemeChecked
    {
        get => settingsService.LoadUserAppTheme() == AppTheme.Dark;
        set
        {
            if (value)
            {
                settingsService.SetAppTheme(AppTheme.Dark);
                OnPropertyChanged();
            }
        }
    }

    public bool IsLightThemeChecked
    {
        get => settingsService.LoadUserAppTheme() == AppTheme.Light;
        set
        {
            if (value)
            {
                settingsService.SetAppTheme(AppTheme.Light);
                OnPropertyChanged();
            }
        }
    }

    public bool IsUnspecifiedThemeChecked
    {
        get => settingsService.LoadUserAppTheme() == AppTheme.Unspecified;
        set
        {
            if (value)
            {
                settingsService.SetAppTheme(AppTheme.Unspecified);
                OnPropertyChanged();
            }
        }
    }
}
