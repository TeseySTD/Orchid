using Orchid.Application.Services;
using Orchid.Core.Models.ValueObjects;

public interface IPaginationCacheService
{
    Task SaveChapterAsync(BookId bookId, PaginationContext context, int index, PageData[] pages);
    Task<PageData[]?> GetChapterAsync(BookId bookId, PaginationContext context, int index);
    bool ChapterExists(BookId bookId, PaginationContext context, int index);
}
