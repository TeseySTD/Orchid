namespace Orchid.Application.Common.Services;

public interface IDiskCacheService
{
    public Task SaveStringAsync(string key, string content);

    public Task SaveBytesAsync(string key, byte[] bytes);

    public Stream GetStream(string key);

    public bool Exists(string key);

    public void Clear();

    Task<(long RemovableBytes, long ExcludedBytes)> GetFolderSizeAsync(string folderName,
        IEnumerable<string>? excludeFileNames = null);

    void ClearFolder(string folderName, IEnumerable<string>? excludeFileNames = null);
}