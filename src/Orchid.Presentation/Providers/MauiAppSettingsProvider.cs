using Orchid.Application.Common.Providers;
using Orchid.Application.Dto;

namespace Orchid.Presentation.Providers;

public class MauiAppSettingsProvider : IAppSettingsProvider
{
    public event Action? OnSettingsChanged;

    public bool OpenLastBookOnStartup
    {
        get => Preferences.Get(nameof(OpenLastBookOnStartup), false);
        set
        {
            if (OpenLastBookOnStartup == value) return;
            Preferences.Set(nameof(OpenLastBookOnStartup), value);
            OnSettingsChanged?.Invoke();
        }
    }

    public int ReaderFontSize
    {
        get => Preferences.Get(nameof(ReaderFontSize), 16);
        set
        {
            if (ReaderFontSize == value) return;
            Preferences.Set(nameof(ReaderFontSize), value);
            OnSettingsChanged?.Invoke();
        }
    }

    public double ReaderLineHeight
    {
        get => Preferences.Get(nameof(ReaderLineHeight), 1.5);
        set
        {
            if (Math.Abs(ReaderLineHeight - value) < 0.01) return;
            Preferences.Set(nameof(ReaderLineHeight), value);
            OnSettingsChanged?.Invoke();
        }
    }

    public AppThemeMode Theme
    {
        get => (AppThemeMode)Preferences.Get(nameof(Theme), (int)AppThemeMode.System);
        set
        {
            if (Theme == value) return;
            Preferences.Set(nameof(Theme), (int)value);
            OnSettingsChanged?.Invoke();
        }
    }

    public string? LastOpenedBookPath
    {
        get => Preferences.Get(nameof(LastOpenedBookPath), null);
        set
        {
            if (LastOpenedBookPath == value) return;
            Preferences.Set(nameof(LastOpenedBookPath), value);
            OnSettingsChanged?.Invoke();
        }
    }

    public string AppLanguage
    {
        get => Preferences.Get(nameof(AppLanguage), "en-US");
        set => Preferences.Set(nameof(AppLanguage), value);
    }
}