using System.Text;
using Microsoft.Extensions.Options;
using Orchid.Application.Common.Providers;
using Orchid.Infrastructure.Data.Providers.Options;

namespace Orchid.Infrastructure.Data.Providers;

public class FileDiskCacheProvider : IDiskCacheProvider
{
    private readonly DiskCacheProviderOptions _options;

    public FileDiskCacheProvider(IOptions<DiskCacheProviderOptions> options)
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

        var excludedSet = excludeFileNames != null
            ? new HashSet<string>(excludeFileNames, StringComparer.OrdinalIgnoreCase)
            : [];

        var trashDirectory = Path.Combine(GetFullCachePath, "trash");

        if (excludedSet.Count == 0)
        {
            try
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
                return;
            }
            catch (IOException)
            {
                // Fallback to granular deletion if the root directory or files inside are locked
            }
        }

        var dirInfo = new DirectoryInfo(path);

        foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            if (!excludedSet.Contains(file.Name))
            {
                try
                {
                    file.Delete();
                }
                catch (IOException)
                {
                    // File is locked by another process (WebView cache). Apply Rename Workaround.
                    try
                    {
                        if (!Directory.Exists(trashDirectory))
                        {
                            Directory.CreateDirectory(trashDirectory);
                        }

                        var tempFilePath = Path.Combine(trashDirectory, $"{Guid.NewGuid()}.tmp");

                        // Moving a locked file releases its original path immediately
                        file.MoveTo(tempFilePath);

                        // Attempt immediate deletion, if it fails, it will remain in trash until restart
                        File.Delete(tempFilePath);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine($"Failed to move locked file '{file.Name}': {ex.Message}");
#endif
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine($"Error deleting file '{file.Name}': {ex.Message}");
#endif
                }
            }
        }

        // Clear empty directories 
        foreach (var dir in dirInfo.EnumerateDirectories("*", SearchOption.AllDirectories).Reverse())
        {
            try
            {
                if (dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0)
                {
                    dir.Delete();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"Skipping locked directory '{dir.Name}': {ex.Message}");
#endif
            }
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