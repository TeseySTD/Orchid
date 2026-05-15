namespace Orchid.Application.Common.Providers;

public interface ISecureStorageProvider
{
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    void Remove(string key);
    void RemoveAll();
}