using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orchid.Application.Common;
using Orchid.Application.Common.Repo;
using Orchid.Infrastructure.Data;
using Orchid.Infrastructure.Data.Repo;

namespace Orchid.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, Action<InfrastructureOptions> configureOptions)
    {
        var builder = new InfrastructureOptions();
        configureOptions(builder);
        services.AddSingleton(Options.Create(builder.DiskCacheServiceOptions));
        
        services.AddTransient<IImagesRepository, ImagesRepository>();
        services.AddSingleton<DiskCacheService, FileDiskCacheService>();
        return services;
    }
}