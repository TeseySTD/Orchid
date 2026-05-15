using Orchid.Infrastructure.Cloud.Options;
using Orchid.Infrastructure.Data.Providers.Options;

namespace Orchid.Infrastructure;

public class InfrastructureOptions
{
    public DiskCacheProviderOptions DiskCacheProviderOptions { get; } = new();
    public JsonStorageProviderOptions JsonStorageProviderOptions { get; } = new();
    
    public GoogleAuthOptions GoogleAuthOptions { get; } = new();
}