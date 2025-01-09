
namespace Alcedo.Services.SettingsService
{
    public interface ISettingsService
    {
        string LoadApiKey();
        AppTheme? LoadUserAppTheme();
        void RefreshAppTheme();
        void SaveApiKey(string apiKey);
        void SetAppTheme(string? themeName);
        void SetAppTheme(AppTheme? appTheme, bool saveToPreferences = true);
    }
}