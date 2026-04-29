using System.Text.Json;
using Google.Apis.Util.Store;
using Orchid.Application.Common.Services;

namespace Orchid.Infrastructure.Cloud.Auth;

public class OAuthDataStoreAdapter : IDataStore
{
    private readonly ILocalSecureStorage _secureStorage;

    public OAuthDataStoreAdapter(ILocalSecureStorage secureStorage)
    {
        _secureStorage = secureStorage;
    }

    public async Task StoreAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _secureStorage.SetAsync(key, json);
    }

    public async Task DeleteAsync<T>(string key)
    {
        _secureStorage.Remove(key);
        await Task.CompletedTask;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _secureStorage.GetAsync(key);

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task ClearAsync()
    {
        _secureStorage.RemoveAll();
        await Task.CompletedTask;
    }
}