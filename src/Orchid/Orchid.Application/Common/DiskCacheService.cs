namespace Orchid.Application.Common;

public abstract class DiskCacheService
{
    public const string CacheFolderName = "OrchidCache";

    public abstract Task SaveStringAsync(string key, string content);

    public abstract Task SaveBytesAsync(string key, byte[] bytes);

    public abstract Stream GetStream(string key);

    public abstract bool Exists(string key);

    public abstract void Clear();
}