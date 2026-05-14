using System.Text;
using Microsoft.Extensions.Options;
using Orchid.Application.Common.Services;
using Orchid.Infrastructure.Data.Services.Options;

namespace Orchid.Infrastructure.Data.Services;

public class FileDiskCacheService : IDiskCacheService
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

    public async Task SaveStringAsync(string key, string content)
    {
        var path = GetPath(key);
        EnsureDirectoryExists(path);
        if (!File.Exists(path))
        {
            await File.WriteAllTextAsync(path, content, Encoding.UTF8);
        }
    }

    public async Task SaveBytesAsync(string key, byte[] content)
    {
        var path = GetPath(key);
        EnsureDirectoryExists(path);
        if (!File.Exists(path))
        {
            await File.WriteAllBytesAsync(path, content);
        }
    }

    public Stream GetStream(string key)
    {
        var path = GetPath(key);
        if (File.Exists(path))
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        }

        return Stream.Null;
    }

    public bool Exists(string key) => File.Exists(GetPath(key));

    public async Task<(long RemovableBytes, long ExcludedBytes)> GetFolderSizeAsync(string folderName,
        IEnumerable<string>? excludeFileNames = null)
    {
        var path = Path.Combine(GetFullCachePath, folderName);
        if (!Directory.Exists(path)) return (0, 0);

        return await Task.Run(() =>
        {
            var dirInfo = new DirectoryInfo(path);
            var excludedSet = excludeFileNames != null
                ? new HashSet<string>(excludeFileNames, StringComparer.OrdinalIgnoreCase)
                : [];

            long removable = 0;
            long excluded = 0;

            foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                if (excludedSet.Contains(file.Name))
                {
                    excluded += file.Length;
                }
                else
                {
                    removable += file.Length;
                }
            }

            return (removable, excluded);
        });
    }

    public void ClearFolder(string folderName, IEnumerable<string>? excludeFileNames = null)
    {
        var path = Path.Combine(GetFullCachePath, folderName);
        if (!Directory.Exists(path)) return;

        try
        {
            var excludedSet = excludeFileNames != null
                ? new HashSet<string>(excludeFileNames, StringComparer.OrdinalIgnoreCase)
                : [];

            if (excludedSet.Count == 0)
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
                return;
            }

            var dirInfo = new DirectoryInfo(path);
            foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                if (!excludedSet.Contains(file.Name))
                {
                    file.Delete();
                }
            }

            foreach (var dir in dirInfo.EnumerateDirectories("*", SearchOption.AllDirectories).Reverse())
            {
                if (dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0)
                {
                    dir.Delete();
                }
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"Error clearing selective folder cache: {ex.Message}");
#endif
        }
    }


    public void Clear()
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

    private string GetFullCachePath => _options.BaseDirectory;
    private string GetPath(string key) => Path.Combine(GetFullCachePath, key);
}