using Orchid.Application.Services;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Services;

public interface IPaginationCacheService
{
    Task SaveMapAsync(BookId bookId, PaginationContext context, Dictionary<int, int> pages);
    
    Task<Dictionary<int, int>?> GetMapAsync(BookId bookId, PaginationContext context);
}