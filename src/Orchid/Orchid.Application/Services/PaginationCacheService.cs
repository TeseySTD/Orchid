using System.Text.Json;
using Orchid.Application.Common.Services;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Services;

public record PaginationContext(double Width, double Height, double FontSize, string FontFamily, double LineHeight);

public class PaginationCacheService : IPaginationCacheService
{
    private readonly DiskCacheService _cache; 
    private const string FolderName = "pagination";
    
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PaginationCacheService(DiskCacheService cache)
    {
        _cache = cache;
    }

    public async Task SaveMapAsync(BookId bookId, PaginationContext context, Dictionary<int, PageData[]> pages)
    {
        var key = GetCacheKey(bookId, context);
        
        var json = JsonSerializer.Serialize(pages, JsonOptions);
        
        await _cache.SaveStringAsync(key, json);
    }

    public async Task<Dictionary<int, PageData[]>?> GetMapAsync(BookId bookId, PaginationContext context)
    {
        var key = GetCacheKey(bookId, context);

        if (!_cache.Exists(key))
        {
            return null;
        }

        try
        {
            await using var stream = _cache.GetStream(key);
            
            return await JsonSerializer.DeserializeAsync<Dictionary<int, PageData[]>>(stream, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private string GetCacheKey(BookId bookId, PaginationContext context)
    {
        var hash = GenerateHash(context);
        return Path.Combine(FolderName, $"{bookId.Value}_{hash}.json");
    }

    private string GenerateHash(PaginationContext ctx)
    {
        var raw = $"{ctx.Width:F1}_{ctx.Height:F1}_{ctx.FontSize}_{ctx.FontFamily}_{ctx.LineHeight}";
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes)[..8]; 
    }
}
