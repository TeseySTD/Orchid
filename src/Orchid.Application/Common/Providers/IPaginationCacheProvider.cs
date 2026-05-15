using Orchid.Application.Dto;
using Orchid.Application.Services;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Providers;

public interface IPaginationCacheProvider
{
    Task SaveChapterAsync(BookId bookId, PaginationContext context, int index, PageData[] pages);
    Task<PageData[]?> GetChapterAsync(BookId bookId, PaginationContext context, int index);
    bool ChapterExists(BookId bookId, PaginationContext context, int index);
    Task<CacheSizeInfo> GetCacheSizeAsync();
    void ClearCache();
}