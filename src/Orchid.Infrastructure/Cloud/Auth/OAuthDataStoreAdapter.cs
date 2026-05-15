using System.Text.Json;
using Google.Apis.Util.Store;
using Orchid.Application.Common.Providers;
using Orchid.Application.Common.Services;

namespace Orchid.Infrastructure.Cloud.Auth;

public class OAuthDataStoreAdapter : IDataStore
{
    private readonly ISecureStorageProvider _secureStorageProvider;

    public OAuthDataStoreAdapter(ISecureStorageProvider secureStorageProvider)
    {
        _secureStorageProvider = secureStorageProvider;
    }

    public async Task StoreAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _secureStorageProvider.SetAsync(key, json);
    }

    public async Task DeleteAsync<T>(string key)
    {
        _secureStorageProvider.Remove(key);
        await Task.CompletedTask;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _secureStorageProvider.GetAsync(key);

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task ClearAsync()
    {
        _secureStorageProvider.RemoveAll();
        await Task.CompletedTask;
    }
}