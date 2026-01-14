using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Orchid.Application;
using Orchid.Engine;
using Orchid.Infrastructure;

namespace Orchid.Presentation;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });


        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();
        
        builder.Services
            .AddEngineServices()
            .AddInfrastructureServices(options =>
            {
                options.DiskCacheServiceOptions.BaseDirectory = FileSystem.Current.CacheDirectory;
            })
            .AddApplicationServices()
            .AddPresentationServices();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
