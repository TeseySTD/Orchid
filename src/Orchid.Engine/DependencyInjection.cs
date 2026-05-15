using Microsoft.Extensions.DependencyInjection;
using Orchid.Application.Common.Engine;
using Orchid.Application.Common.Providers;
using Orchid.Application.Common.Services;
using Orchid.Engine.Epub;

namespace Orchid.Engine;

public static class DependencyInjection
{
    public static IServiceCollection AddEngineServices(this IServiceCollection services)
    {
        services.AddSingleton<IBookParserFactory, BookParserFactory>();
        services.AddScoped<IBookParser, EpubBookParser>();

        return services;
    }
}