using System.Text.RegularExpressions;
using Orchid.Application.Common.Repo;
using Orchid.Application.Common.Services;
using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;
using Orchid.Engine.Common;

namespace Orchid.Application.Services;

public class BookResourcesService(
    IBookServiceProvider bookServiceProvider,
    IImagesRepository imagesRepository)
    : IBookResourcesService
{
    public async Task<Book> ReadBookAsync(string bookPath)
    {
        var bookService = bookServiceProvider.GetService(bookPath);
        var book = await bookService.ReadAsync(bookPath);

        if (book.Cover != null)
        {
            book.Cover.Path = Path.Combine(book.Metadata.FileName, book.Cover.Name);
            await imagesRepository.SaveImageAsync(
                Image.Create(
                    book.Cover.Path, 
                    book.Cover.Data
                )
            );
        }

        await foreach (var img in bookService.GetBookImagesAsync(bookPath))
        {
            await imagesRepository.SaveImageAsync(
                Image.Create(
                    Path.Combine(book.Metadata.FileName, img.Name),
                    img.Data
                )
            );
        }

        return book;
    }

    public async Task<Chapter> ReadChapterAsync(string bookPath, int chapterIndex)
    {
        var bookService = bookServiceProvider.GetService(bookPath);
        var bookFileName = Path.GetFileName(bookPath);

        var bookChapter = await bookService.ReadChapterAsync(bookPath, chapterIndex);
        bookChapter = Chapter.Create(bookChapter.Title, ProcessHtmlImages(bookChapter.Html, bookFileName));
        return bookChapter;
    }

    public async Task<List<Chapter>> ReadChaptersAsync(string bookPath)
    {
        var bookService = bookServiceProvider.GetService(bookPath);
        var bookFileName = Path.GetFileName(bookPath);

        var chapters = await bookService.ReadChaptersAsync(bookPath);

        return chapters
            .Select(c => Chapter.Create(c.Title, ProcessHtmlImages(c.Html, bookFileName)))
            .ToList();
    }

    private string ProcessHtmlImages(string html, string bookFileName)
    {
        html = Regex.Replace(html, @"(xlink:href="")([^""]+)("")", match =>
        {
            var imageName = match.Groups[2].Value;
            return match.Groups[1] + Path.Combine(bookFileName, imageName) + match.Groups[3].Value;
        });

        html = Regex.Replace(html, @"(<img[^>]+src\s*=\s*"")([^""]+)(""[^>]*>)", match =>
        {
            var imageName = match.Groups[2].Value;
            return match.Groups[1].Value + Path.Combine(bookFileName, imageName) + match.Groups[3].Value;
        });

        var matchBody = Regex.Match(html, @"<body.*?>(.*)</body>", RegexOptions.Singleline);
        return matchBody.Success ? $"<body>{matchBody.Groups[1].Value}</body>" : html;
    }

    public async Task<IEnumerable<CssFile>> GetBookCssFilesAsync(string bookPath)
    {
        var bookService = bookServiceProvider.GetService(bookPath);
        return await bookService.GetBookCssAsync(bookPath);
    }
}