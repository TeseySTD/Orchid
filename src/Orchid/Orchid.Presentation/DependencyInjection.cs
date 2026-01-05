using Microsoft.Extensions.Http;
using Orchid.Application.Common;
using Orchid.Presentation.Services;

namespace Orchid.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddSingleton<BookPaginationService>();
        return services;
    }
}