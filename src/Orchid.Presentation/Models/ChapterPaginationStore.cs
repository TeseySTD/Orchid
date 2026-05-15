using System.Collections.Concurrent;
using Orchid.Application.Common.Providers;
using Orchid.Application.Dto;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Presentation.Models;

public class ChapterPaginationStore
{
    private readonly IPaginationCacheProvider _cacheProvider;
    private readonly ConcurrentDictionary<int, PageData[]> _memoryCache = new();

    public BookId? CurrentBookId { get; private set; }
    public PaginationContext? CurrentContext { get; private set; }

    public ChapterPaginationStore(IPaginationCacheProvider cacheProvider) => _cacheProvider = cacheProvider;

    public void Init(BookId bookId, PaginationContext context)
    {
        CurrentBookId = bookId;
        CurrentContext = context;
        _memoryCache.Clear();
    }

    public async Task<PageData[]> GetChapterPagesAsync(int index)
    {
        if (CurrentBookId == null || CurrentContext == null) return [];

        if (_memoryCache.TryGetValue(index, out var pages))
        {
            _ = PrefetchNeighbors(index);
            return pages;
        }

        var diskPages = await _cacheProvider.GetChapterAsync(CurrentBookId, CurrentContext, index) ?? [];
        AddToMemory(index, diskPages);

        ManageWindow(index);
        _ = PrefetchNeighbors(index);

        return diskPages;
    }

    private void AddToMemory(int index, PageData[] pages)
    {
        _memoryCache[index] = pages;
    }

    private async Task PrefetchNeighbors(int centerIndex)
    {
        if (CurrentBookId == null || CurrentContext == null) return;

        foreach (var i in new[] { centerIndex - 1, centerIndex + 1 })
        {
            if (i < 0 || _memoryCache.ContainsKey(i)) continue;

            var pages = await _cacheProvider.GetChapterAsync(CurrentBookId, CurrentContext, i);
            if (pages != null)
            {
                _memoryCache[i] = pages;
                ManageWindow(centerIndex);
            }
        }
    }

    public void Invalidate(int index)
    {
        _memoryCache.TryRemove(index, out _);
    }

    private void ManageWindow(int centerIndex)
    {
        var keysToRemove = _memoryCache.Keys
            .Where(idx => idx < centerIndex - 1 || idx > centerIndex + 1)
            .ToList();

        foreach (var key in keysToRemove) _memoryCache.TryRemove(key, out _);
    }

    public void Clear() => _memoryCache.Clear();
}