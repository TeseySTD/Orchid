using System.Text.Json;
using Microsoft.Extensions.Options;
using Orchid.Application.Common.Services;
using Orchid.Infrastructure.Data.Services.Options;

namespace Orchid.Infrastructure.Data.Services;

public class JsonStorageService : IJsonStorageService
{
    private readonly string _basePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonStorageService(IOptions<JsonStorageServiceOptions> options)
    {
        if (string.IsNullOrEmpty(options.Value.StoragePath))
            throw new ArgumentException($"{nameof(options.Value.StoragePath)} is missing.", nameof(options));
        _basePath = options.Value.StoragePath;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public async Task SaveAsync<T>(string key, T value)
    {
        var path = GetPath(key);
        EnsureDirectory(path);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value, _jsonOptions);
    }

    public async Task<T?> LoadAsync<T>(string key)
    {
        var path = GetPath(key);
        if (!File.Exists(path)) return default;
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
        }
        catch
        {
            return default;
        }
    }

    public void Delete(string key)
    {
        var path = GetPath(key);
        if (File.Exists(path)) File.Delete(path);
    }

    public bool Exists(string key) => File.Exists(GetPath(key));

    public async Task<IEnumerable<T>> LoadAllInFolderAsync<T>(string folderName)
    {
        var folderPath = Path.Combine(_basePath, folderName);
        if (!Directory.Exists(folderPath)) return Enumerable.Empty<T>();

        var result = new List<T>();
        foreach (var file in Directory.GetFiles(folderPath, "*.json"))
        {
            try
            {
                await using var stream = File.OpenRead(file);
                var item = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
                if (item != null) result.Add(item);
            }
            catch
            {
                // ignored
            }
        }

        return result;
    }

    private string GetPath(string key)
    {
        var fullPath = Path.Combine(_basePath, key);
        if (!fullPath.EndsWith(".json")) fullPath += ".json";
        return fullPath;
    }

    private void EnsureDirectory(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }
}