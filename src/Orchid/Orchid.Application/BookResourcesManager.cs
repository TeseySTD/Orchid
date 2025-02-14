using System.Text.RegularExpressions;
using Orchid.Application.Common;
using Orchid.Application.Common.Repo;
using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;
using Orchid.Engine.Common;

namespace Orchid.Application;

public class BookResourcesManager : IBookResourcesManager
{
    private readonly IBookServiceProvider _bookServiceProvider;
    private readonly IImagesRepository _imagesRepository;

    public BookResourcesManager(IBookServiceProvider bookServiceProvider,
        IImagesRepository imagesRepository)
    {
        _bookServiceProvider = bookServiceProvider;
        _imagesRepository = imagesRepository;
    }

    public async Task<Book> ReadBookAsync(string bookPath, string cacheDirectoryPath)
    {
        var bookService = _bookServiceProvider.GetService(bookPath);
        var book = await bookService.ReadAsync(bookPath);
        
        if(book.Cover != null)
            book.Cover.Path = Path.Combine(book.Metadata.FileName, book.Cover.Path);
        var bookImages = await GetBookImagesAsync(bookPath);
        
        await _imagesRepository.SaveImagesAsync(bookImages, cacheDirectoryPath);
        
        return book;
    }
    
    public async Task<Chapter> ReadChapterAsync(string bookPath, int chapterIndex)
    {
        var bookService = _bookServiceProvider.GetService(bookPath);
        var book = await bookService.ReadAsync(bookPath);
        
        var bookChapter = await bookService.ReadChapterAsync(bookPath, chapterIndex);
        bookChapter = Chapter.Create(bookChapter.Title, ProcessHtmlImages(bookChapter.Html, book.Metadata.FileName));
        return bookChapter;
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
        var bookService = _bookServiceProvider.GetService(bookPath);
        return await bookService.GetBookCssAsync(bookPath);
    }

    public async Task<IEnumerable<Image>> GetBookImagesAsync(string bookPath)
    {
        var bookService = _bookServiceProvider.GetService(bookPath);
        var book = await bookService.ReadAsync(bookPath);
        
        var bookImages = (await bookService.GetBookImagesAsync(bookPath)).ToList();
        if(book.Cover != null)
            bookImages.Add(Image.Create(book.Cover.Path, book.Cover.ImageData));
        
        var result = bookImages.Select(i =>
            Image.Create(
                Path.Combine(Path.GetFileName(bookPath), i.Name),
                i.Data
            )
        );
        return result;
    }
}