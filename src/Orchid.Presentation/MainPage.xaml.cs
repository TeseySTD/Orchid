
using Orchid.Application.Models;

namespace Orchid.Presentation;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        ApplyNativeBackgroundColor();

        if (Microsoft.Maui.Controls.Application.Current != null)
        {
            Microsoft.Maui.Controls.Application.Current.RequestedThemeChanged += (_, _) => ApplyNativeBackgroundColor();
        }
    }

    private void ApplyNativeBackgroundColor()
    {
        var theme = (AppThemeMode)Preferences.Get("Theme", (int)AppThemeMode.System);

        bool isDark = theme switch
        {
            AppThemeMode.Dark => true,
            AppThemeMode.Light => false,
            _ => Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark
        };

        var bgColor = isDark ? Color.FromArgb("#1C1012") : Color.FromArgb("#FFF8F7");

        BackgroundColor = bgColor;
    }
}
