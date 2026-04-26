using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orchid.Application.Common.Repo;
using Orchid.Application.Common.Services;
using Orchid.Infrastructure.Data.Repo;
using Orchid.Infrastructure.Data.Services;

namespace Orchid.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, Action<InfrastructureOptions> configureOptions)
    {
        var builder = new InfrastructureOptions();
        configureOptions(builder);
        services.AddSingleton(Options.Create(builder.DiskCacheServiceOptions));
        services.AddSingleton(Options.Create(builder.JsonStorageServiceOptions));
        
        services.AddTransient<IImagesRepository, ImagesRepository>();
        services.AddSingleton<DiskCacheService, FileDiskCacheService>();
        services.AddSingleton<IJsonStorageService, JsonStorageService>();
        return services;
    }
}