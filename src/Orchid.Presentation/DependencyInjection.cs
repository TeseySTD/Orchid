using MudBlazor;
using MudBlazor.Services;
using Orchid.Application.Common.Providers;
using Orchid.Presentation.Providers;
using Orchid.Presentation.Services;

namespace Orchid.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddSingleton<BookPaginationService>();
        services.AddSingleton<LocalizationStateService>();
        services.AddSingleton<ISecureStorageProvider, MauiSecureStorageProvider>();
        services.AddSingleton<IAppSettingsProvider, MauiAppSettingsProvider>();
        services.AddSingleton<IWebAuthenticatorProvider, MauiWebAuthenticatorProvider>();
        services.AddSingleton<IPlatformEnvironmentProvider, MauiPlatformEnvironmentProvider>();
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
        });
        services.AddTransient<MainPage>();
        services.AddLocalization();
        return services;
    }
}