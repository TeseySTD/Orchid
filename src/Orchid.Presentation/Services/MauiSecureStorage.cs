using Orchid.Application.Common.Services;

namespace Orchid.Presentation.Services;

public class MauiSecureStorage : ILocalSecureStorage
{
    public Task SetAsync(string key, string value) => SecureStorage.Default.SetAsync(key, value);

    public Task<string?> GetAsync(string key) => SecureStorage.Default.GetAsync(key);

    public void Remove(string key) => SecureStorage.Default.Remove(key);

    public void RemoveAll() => SecureStorage.Default.RemoveAll();
}