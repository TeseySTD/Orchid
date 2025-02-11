using Microsoft.Extensions.Logging;
using Orchid.Application;
using Orchid.Engine;
using Orchid.Infrastructure;
using FileSystem = Microsoft.VisualBasic.FileIO.FileSystem;

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
		builder.Services
			.AddEngineServices()
			.AddInfrastructureServices()
			.AddApplicationServices()
			.AddPresentationServices();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
