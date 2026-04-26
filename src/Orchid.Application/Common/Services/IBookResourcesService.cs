using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Services;

public interface IBookResourcesService
{
    public Task<Book> ReadBookAsync(string bookPath);

    public Task<IEnumerable<CssFile>> GetBookCssFilesAsync(string bookPath);

    public Task<List<Chapter>> ReadChaptersAsync(string bookPath);
}