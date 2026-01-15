using Orchid.Application.Common.Services;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Services;

public record PaginationContext(double Width, double Height, double FontSize, string FontFamily, double LineHeight);

public class PaginationCacheService : IPaginationCacheService
{
    private readonly IJsonStorageService _storage;
    private const string FolderName = "pagination";

    public PaginationCacheService(IJsonStorageService storage)
    {
        _storage = storage;
    }

    public async Task SaveMapAsync(BookId bookId, PaginationContext context, Dictionary<int, int> pages)
    {
        var hash = GenerateHash(context);
        var key = Path.Combine(FolderName, $"{bookId.Value}_{hash}");
        await _storage.SaveAsync(key, pages);
    }

    public async Task<Dictionary<int, int>?> GetMapAsync(BookId bookId, PaginationContext context)
    {
        var hash = GenerateHash(context);
        var key = Path.Combine(FolderName, $"{bookId.Value}_{hash}");
        return await _storage.LoadAsync<Dictionary<int, int>>(key);
    }

    private string GenerateHash(PaginationContext ctx)
    {
        var raw = $"{ctx.Width:F1}_{ctx.Height:F1}_{ctx.FontSize}_{ctx.FontFamily}_{ctx.LineHeight}";
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes)[..8]; 
    }
}