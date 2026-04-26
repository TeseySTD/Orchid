using System.Text.RegularExpressions;
using Orchid.Application.Common.Repo;
using Orchid.Application.Common.Services;
using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;

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
            await imagesRepository.SaveImageAsync(
                book.Id,
                book.Cover
            );
            var relativePath = imagesRepository.GetRelativeImagePath(book.Id, book.Cover.Name);

            book.UpdateCoverPath(relativePath);
        }

        await foreach (var img in bookService.GetBookImagesAsync(bookPath))
        {
            await imagesRepository.SaveImageAsync(book.Id, img);
        }

        return book;
    }

    public async Task<List<Chapter>> ReadChaptersAsync(string bookPath, BookId bookId)
    {
        var bookService = bookServiceProvider.GetService(bookPath);
        var chapters = await bookService.ReadChaptersAsync(bookPath);

        return chapters
            .Select(c => Chapter.Create(c.Title, ProcessHtmlImagesLinks(c.Html, bookId)))
            .ToList();
    }

    private string ProcessHtmlImagesLinks(string html, BookId bookId)
    {
        html = Regex.Replace(html, @"(xlink:href="")([^""]+)("")", match =>
        {
            var imageName = match.Groups[2].Value;
            var imageLink = imagesRepository.GetRelativeImagePath(bookId, imageName);
            return match.Groups[1] + imageLink + match.Groups[3].Value;
        });

        html = Regex.Replace(html, @"(<img[^>]+src\s*=\s*"")([^""]+)(""[^>]*>)", match =>
        {
            var imageName = match.Groups[2].Value;
            var imageLink = imagesRepository.GetRelativeImagePath(bookId, imageName);
            return match.Groups[1].Value + imageLink + match.Groups[3].Value;
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