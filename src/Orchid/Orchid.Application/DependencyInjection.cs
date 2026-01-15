using Microsoft.Extensions.DependencyInjection;
using Orchid.Application.Common;
using Orchid.Application.Services;

namespace Orchid.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IBookResourcesManager, BookResourcesManager>();
        services.AddTransient<PaginationCacheService>();

        return services;
    }
}