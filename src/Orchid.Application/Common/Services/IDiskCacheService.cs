namespace Orchid.Application.Common.Services;

public interface IDiskCacheService
{
    public Task SaveStringAsync(string key, string content);

    public Task SaveBytesAsync(string key, byte[] bytes);

    public Stream GetStream(string key);

    public bool Exists(string key);

    public void Clear();
}