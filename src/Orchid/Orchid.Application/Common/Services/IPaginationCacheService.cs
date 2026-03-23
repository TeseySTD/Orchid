using Orchid.Application.Services;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Services;

public interface IPaginationCacheService
{
    Task SaveMapAsync(BookId bookId, PaginationContext context, Dictionary<int, PageData[]> pages);
    
    Task<Dictionary<int, PageData[]>?> GetMapAsync(BookId bookId, PaginationContext context);
}
