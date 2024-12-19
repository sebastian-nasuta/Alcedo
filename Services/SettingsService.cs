namespace Alcedo.Services;

internal class SettingsService
{
    internal static AppTheme? LoadUserAppTheme()
    {
        var savedTheme = Preferences.Get("UserAppTheme", AppTheme.Unspecified.ToString());
        Enum.TryParse(typeof(AppTheme), savedTheme, out var currentTheme);
        return (AppTheme?)currentTheme;
    }

    internal static void SetAppTheme(string? themeName)
    {
        Enum.TryParse(typeof(AppTheme), themeName, out var appTheme);
        SetAppTheme((AppTheme?)appTheme);
    }

    internal static void SetAppTheme(AppTheme? appTheme, bool saveToPreferences = true)
    {
        if (Application.Current is not null)
            Application.Current.UserAppTheme = appTheme ?? AppTheme.Unspecified;

        if (saveToPreferences)
            Preferences.Set("UserAppTheme", appTheme.ToString());
    }

    internal static void RefreshAppTheme() => SetAppTheme(LoadUserAppTheme(), false);
}