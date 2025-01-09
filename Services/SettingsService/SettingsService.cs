using Microsoft.Extensions.Configuration;

namespace Alcedo.Services.SettingsService;

internal class SettingsService(IConfiguration configuration) : ISettingsService
{
    public AppTheme? LoadUserAppTheme()
    {
        var savedTheme = Preferences.Get("UserAppTheme", AppTheme.Unspecified.ToString());
        Enum.TryParse(typeof(AppTheme), savedTheme, out var currentTheme);
        return (AppTheme?)currentTheme;
    }

    public void SetAppTheme(string? themeName)
    {
        Enum.TryParse(typeof(AppTheme), themeName, out var appTheme);
        SetAppTheme((AppTheme?)appTheme);
    }

    public void SetAppTheme(AppTheme? appTheme, bool saveToPreferences = true)
    {
        if (Application.Current is not null)
            Application.Current.UserAppTheme = appTheme ?? AppTheme.Unspecified;

        if (saveToPreferences)
            Preferences.Set("UserAppTheme", appTheme.ToString());
    }

    public void RefreshAppTheme() => SetAppTheme(LoadUserAppTheme(), false);

    public string LoadApiKey()
    {
        var apiKey = Preferences.Get("OpenAI:ApiKey", null);
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = configuration["OpenAI:ApiKey"]
                ?? throw new ArgumentNullException("OpenAI:ApiKey is missing in the configuration file.");

            if (!string.IsNullOrEmpty(apiKey))
            {
                SaveApiKey(apiKey);
            }
        }
        return apiKey;
    }

    public void SaveApiKey(string apiKey)
    {
        Preferences.Set("OpenAI:ApiKey", apiKey);
    }
}
