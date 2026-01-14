using Orchid.Infrastructure.Data;

namespace Orchid.Infrastructure;

public class InfrastructureOptions
{
    public DiskCacheServiceOptions DiskCacheServiceOptions { get; } = new();
}