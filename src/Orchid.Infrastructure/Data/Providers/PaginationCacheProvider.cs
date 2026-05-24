using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Orchid.Application.Common.Providers;
using Orchid.Application.Dto;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Infrastructure.Data.Providers;

public class PaginationCacheProvider : IPaginationCacheProvider
{
    private readonly IDiskCacheProvider _cache;
    private const string FolderName = "pagination";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PaginationCacheProvider(IDiskCacheProvider cache) => _cache = cache;


    public async Task SaveChapterAsync(BookId bookId, PaginationContext context, int index, PageData[] pages)
    {
        var key = GetChapterKey(bookId, context, index);

        using var memoryStream = new MemoryStream();
        await using (var brotliStream = new BrotliStream(memoryStream, CompressionLevel.Fastest))
        {
            await JsonSerializer.SerializeAsync(brotliStream, pages, JsonOptions);
        }

        var compressedBytes = memoryStream.ToArray();
        var base64String = Convert.ToBase64String(compressedBytes);

        await _cache.SaveStringAsync(key, base64String);
    }


    public async Task<PageData[]?> GetChapterAsync(BookId bookId, PaginationContext context, int index)
    {
        var key = GetChapterKey(bookId, context, index);
        if (!_cache.Exists(key)) return null;

        try
        {
            await using var stream = _cache.GetStream(key);
            using var reader = new StreamReader(stream);
            var base64String = await reader.ReadToEndAsync();

            var compressedBytes = Convert.FromBase64String(base64String);
            using var inputStream = new MemoryStream(compressedBytes);
            await using var brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress);

            return await JsonSerializer.DeserializeAsync<PageData[]>(brotliStream, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public bool ChapterExists(BookId bookId, PaginationContext context, int index)
        => _cache.Exists(GetChapterKey(bookId, context, index));

    public bool IsChapterValid(BookId bookId, PaginationContext context, int index, long expectedSize)
    {
        var key = GetChapterKey(bookId, context, index);
        if (!_cache.Exists(key)) return false;

        try
        {
            using var stream = _cache.GetStream(key);
            return stream.Length == expectedSize && stream.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<CacheSizeInfo> GetCacheSizeAsync()
    {
        var sizes = await _cache.GetFolderSizeAsync(FolderName);
        return new CacheSizeInfo(sizes.RemovableBytes, sizes.ExcludedBytes);
    }

    public void ClearCache() => _cache.ClearFolder(FolderName);

    public async Task<Dictionary<string, int>?> GetManifestAsync(BookId bookId, PaginationContext context)
    {
        var key = GetManifestKey(bookId, context);
        if (!_cache.Exists(key)) return null;

        try
        {
            await using var stream = _cache.GetStream(key);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<Dictionary<string, int>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveManifestAsync(BookId bookId, PaginationContext context, Dictionary<string, int> manifest)
    {
        var key = GetManifestKey(bookId, context);
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        await _cache.SaveStringAsync(key, json);
    }

    private string GetChapterKey(BookId bookId, PaginationContext context, int index)
    {
        var hash = GenerateHash(context);
        return Path.Combine(FolderName, $"{bookId.Value}_{hash}", $"ch_{index}.br");
    }

    private string GetManifestKey(BookId bookId, PaginationContext context)
    {
        var hash = GenerateHash(context);
        return Path.Combine(FolderName, $"{bookId.Value}_{hash}", "manifest.json");
    }

    private string GenerateHash(PaginationContext ctx)
    {
        var raw = $"{ctx.Width:F1}_{ctx.Height:F1}_{ctx.FontSize}_{ctx.FontFamily}_{ctx.LineHeight}";
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes)[..8];
    }
}