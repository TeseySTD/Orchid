using System.Reflection;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Orchid.Application;
using Orchid.Engine;
using Orchid.Infrastructure;
using Orchid.Infrastructure.Cloud.Options;

namespace Orchid.Presentation;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        // Make webview transparent
        BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping("TransparentBackground", (handler, _) =>
        {
#if WINDOWS
        handler.PlatformView.DefaultBackgroundColor = Microsoft.UI.Colors.Transparent;
#elif ANDROID
        handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
            handler.PlatformView.Opaque = false;
#endif
        });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("Orchid.Presentation.appsettings.secrets.json");
        if (stream != null)
        {
            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            builder.Configuration.AddConfiguration(config);
        }

        builder.Services
            .AddEngineServices()
            .AddInfrastructureServices(options =>
            {
                options.DiskCacheProviderOptions.BaseDirectory = FileSystem.Current.CacheDirectory;
                options.JsonStorageProviderOptions.StoragePath = FileSystem.Current.AppDataDirectory;
                builder.Configuration
                    .GetSection(GoogleAuthOptions.SectionName)
                    .Bind(options.GoogleAuthOptions);
#if ANDROID
        options.GoogleAuthOptions.ClientId = options.GoogleAuthOptions.AndroidClientId;
#elif IOS
                options.GoogleAuthOptions.ClientId = options.GoogleAuthOptions.IosClientId;
#elif MACCATALYST || WINDOWS
        options.GoogleAuthOptions.ClientId = options.GoogleAuthOptions.DesktopClientId;
        options.GoogleAuthOptions.ClientSecret = options.GoogleAuthOptions.DesktopClientSecret;
#endif
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