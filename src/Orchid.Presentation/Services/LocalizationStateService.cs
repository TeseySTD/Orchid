using System.Globalization;
using Orchid.Application.Common.Providers;

namespace Orchid.Presentation.Services;

public class LocalizationStateService
{
    private readonly IAppSettingsProvider _appSettings;

    public event Action? OnLanguageChanged;
    
    public string CurrentLanguage { get; private set; } = "en-US";

    public LocalizationStateService(IAppSettingsProvider appSettings)
    {
        _appSettings = appSettings;
    }

    public void Initialize()
    {
        CurrentLanguage = _appSettings.AppLanguage;
        
        if (string.IsNullOrEmpty(CurrentLanguage))
        {
            CurrentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.StartsWith("uk") 
                ? "uk-UA" 
                : "en-US";
                
            _appSettings.AppLanguage = CurrentLanguage;
        }

        ApplyCulture(CurrentLanguage);
    }

    public void SetLanguage(string cultureCode)
    {
        if (CurrentLanguage == cultureCode) return;

        CurrentLanguage = cultureCode;
        _appSettings.AppLanguage = cultureCode;
        
        ApplyCulture(cultureCode);
        
        OnLanguageChanged?.Invoke();
    }

    private void ApplyCulture(string cultureCode)
    {
        var culture = new CultureInfo(cultureCode);

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        
        // Ensure the current Blazor dispatcher thread is also updated immediately
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Resources.Translations.AppResources.Culture = culture;
    }
}
