namespace Orchid.Application.Common.Services;

public interface ILocalSecureStorage
{
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    void Remove(string key);
    void RemoveAll();
}