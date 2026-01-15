using Microsoft.Extensions.DependencyInjection;
using Orchid.Application.Common.Services;
using Orchid.Application.Services;

namespace Orchid.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IBookResourcesService, BookResourcesService>();
        services.AddTransient<IPaginationCacheService,PaginationCacheService>();

        return services;
    }
}