using MudBlazor;
using MudBlazor.Services;
using Orchid.Application.Common.Services;
using Orchid.Presentation.Services;

namespace Orchid.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddSingleton<BookPaginationService>();
        services.AddSingleton<ILocalSecureStorage, MauiSecureStorage>();
        services.AddSingleton<IAppSettingsService, MauiAppSettingsService>();
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
        });
        return services;
    }
}