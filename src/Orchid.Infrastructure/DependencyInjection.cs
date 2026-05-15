using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orchid.Application.Common.Providers;
using Orchid.Application.Common.Repo;
using Orchid.Application.Common.Services;
using Orchid.Infrastructure.Cloud;
using Orchid.Infrastructure.Cloud.Options;
using Orchid.Infrastructure.Data.Providers;
using Orchid.Infrastructure.Data.Repo;

namespace Orchid.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        Action<InfrastructureOptions> configureOptions)
    {
        var builder = new InfrastructureOptions();
        configureOptions(builder);
        services.AddSingleton(Options.Create(builder.DiskCacheProviderOptions));
        services.AddSingleton(Options.Create(builder.JsonStorageProviderOptions));

        services.AddTransient<IImagesRepository, ImagesRepository>();
        services.AddSingleton<IDiskCacheProvider, FileDiskCacheProvider>();
        services.AddSingleton<IJsonStorageProvider, JsonStorageProvider>();
        services.AddTransient<IPaginationCacheProvider, PaginationCacheProvider>();

        services.AddSingleton<ICloudStorageProvider>(sp =>
        {
            var options = builder.GoogleAuthOptions;
            var secureStorage = sp.GetRequiredService<ISecureStorageProvider>();

            return new GoogleDriveProvider(options.ClientId, options.ClientSecret, secureStorage);
        });
        return services;
    }
}