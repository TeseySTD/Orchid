using Orchid.Application.Common.Services;
using Orchid.Presentation.Services;

namespace Orchid.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddSingleton<BookPaginationService>();
        services.AddSingleton<ILocalSecureStorage, MauiSecureStorage>();
        return services;
    }
}