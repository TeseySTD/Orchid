using Orchid.Application.Dto;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Providers;

public interface IPaginationCacheProvider
{
    Task SaveChapterAsync(BookId bookId, PaginationContext context, int index, PageData[] pages);
    Task<PageData[]?> GetChapterAsync(BookId bookId, PaginationContext context, int index);
    bool ChapterExists(BookId bookId, PaginationContext context, int index);
    bool IsChapterValid(BookId bookId, PaginationContext context, int index, long expectedSize);
    Task<CacheSizeInfo> GetCacheSizeAsync();
    void ClearCache();
    Task<Dictionary<string, int>?> GetManifestAsync(BookId bookId, PaginationContext context);
    Task SaveManifestAsync(BookId bookId, PaginationContext context, Dictionary<string, int> manifest);
}