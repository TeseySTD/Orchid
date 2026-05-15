using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Engine;

public interface IBookParser
{
    Task<Book> ReadAsync(string bookFilePath);
    Task<Chapter> ReadChapterAsync(string bookFilePath, int chapterIndex);
    Task<List<Chapter>> ReadChaptersAsync(string bookFilePath);

    Task<IEnumerable<CssFile>> GetBookCssAsync(string bookFilePath);
    IAsyncEnumerable<Image> GetBookImagesAsync(string bookFilePath);
}
