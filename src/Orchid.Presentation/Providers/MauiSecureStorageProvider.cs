using Orchid.Application.Common.Providers;

namespace Orchid.Presentation.Providers;

public class MauiSecureStorageProvider : ISecureStorageProvider
{
    public Task SetAsync(string key, string value) => SecureStorage.Default.SetAsync(key, value);

    public Task<string?> GetAsync(string key) => SecureStorage.Default.GetAsync(key);

    public void Remove(string key) => SecureStorage.Default.Remove(key);

    public void RemoveAll() => SecureStorage.Default.RemoveAll();
}