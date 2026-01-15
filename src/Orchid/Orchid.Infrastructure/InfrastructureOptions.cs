using Orchid.Infrastructure.Data.Services.Options;

namespace Orchid.Infrastructure;

public class InfrastructureOptions
{
    public DiskCacheServiceOptions DiskCacheServiceOptions { get; } = new();
    public JsonStorageServiceOptions JsonStorageServiceOptions { get; } = new();
}