using Microsoft.Extensions.DependencyInjection;
using Orchid.Application.Common;

namespace Orchid.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IBookResourcesManager, BookResourcesManager>();

        return services;
    }
}