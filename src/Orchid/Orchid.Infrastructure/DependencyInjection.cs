using Microsoft.Extensions.DependencyInjection;
using Orchid.Application.Common.Repo;
using Orchid.Infrastructure.Data.Repo;

namespace Orchid.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddTransient<IImagesRepository, ImagesRepository>();
        return services;
    }
}