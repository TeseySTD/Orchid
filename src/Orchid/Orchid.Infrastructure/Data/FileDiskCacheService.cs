using System.Text;
using Orchid.Application.Common;
using Microsoft.Extensions.Options;

namespace Orchid.Infrastructure.Data;

public class FileDiskCacheService : DiskCacheService
{
    private readonly DiskCacheServiceOptions _options;

    public FileDiskCacheService(IOptions<DiskCacheServiceOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrEmpty(_options.BaseDirectory))
        {
            throw new ArgumentException($"'{nameof(_options.BaseDirectory)}' cannot be null or empty.",
                nameof(options));
        }

        if (!Directory.Exists(GetFullCachePath))
        {
            Directory.CreateDirectory(GetFullCachePath);
        }
    }

    public override async Task SaveStringAsync(string key, string content)
    {
        var path = GetPath(key);
        EnsureDirectoryExists(path);
        if (!File.Exists(path))
        {
            await File.WriteAllTextAsync(path, content, Encoding.UTF8);
        }
    }

    public override async Task SaveBytesAsync(string key, byte[] content)
    {
        var path = GetPath(key);
        EnsureDirectoryExists(path);
        if (!File.Exists(path))
        {
            await File.WriteAllBytesAsync(path, content);
        }
    }

    public override Stream GetStream(string key)
    {
        var path = GetPath(key);
        if (File.Exists(path))
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        }

        return Stream.Null;
    }

    public override bool Exists(string key) => File.Exists(GetPath(key));

    public override void Clear()
    {
        if (Directory.Exists(GetFullCachePath))
        {
            try
            {
                Directory.Delete(GetFullCachePath, true);
                Directory.CreateDirectory(GetFullCachePath);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"Error clearing cache: {ex.Message}");
#endif
            }
        }
    }

    private void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private string GetFullCachePath => Path.Combine(_options.BaseDirectory, CacheFolderName);
    private string GetPath(string key) => Path.Combine(GetFullCachePath, key);
}