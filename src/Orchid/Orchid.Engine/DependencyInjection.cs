using Microsoft.Extensions.DependencyInjection;
using Orchid.Application.Common.Services;
using Orchid.Engine.Epub;

namespace Orchid.Engine;

public static class DependencyInjection
{
    public static IServiceCollection AddEngineServices(this IServiceCollection services)
    {
        services.AddSingleton<IBookServiceProvider, BookServiceProvider>();
        services.AddScoped<IBookService, EpubBookService>();

        return services;
    }
}