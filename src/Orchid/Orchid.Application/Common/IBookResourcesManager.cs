using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common;

public interface IBookResourcesManager
{
    public Task<Book> ReadBookAsync(string bookPath);

    public Task<IEnumerable<CssFile>> GetBookCssFilesAsync(string bookPath);
    public Task<IEnumerable<Image>> GetBookImagesAsync(string bookPath);

    public Task<Chapter> ReadChapterAsync(string bookPath, int chapterIndex);
    public Task<List<Chapter>> ReadChaptersAsync(string bookPath);
}