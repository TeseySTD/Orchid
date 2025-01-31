using System.Text.RegularExpressions;
using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;
using Orchid.Engine.Common;
using VersOne.Epub;

namespace Orchid.Engine.Epub;

public class EpubBookService : IBookService
{
    public async Task<Book> ReadAsync(string bookFilePath)
    {
        EpubBook epubBook = await EpubReader.ReadBookAsync(bookFilePath);
        BookTitle title = BookTitle.Create(epubBook.Title);
        List<Author> authors = epubBook.AuthorList.Select(Author.Create).ToList();
        Cover cover = Cover.Create(epubBook.CoverImage);
        Genre genre = Genre.Create("undefined");
        string? language = epubBook.Schema.Package.Language;
        int pageCount = epubBook.ReadingOrder.Count;

        BookMetadata bookMetadata = BookMetadata.Create(
            language: language,
            chaptersCount: pageCount,
            genres: [genre],
            bookFilePath
        );

        PublishingInfo publishingInfo = PublishingInfo.Create(
            publicationDate: epubBook.Schema.Package.Metadata.Dates[0].Date,
            publisher: epubBook.Schema.Package.Metadata.Publishers[0].Publisher,
            null
        );

        List<Chapter> chapters = new List<Chapter>();
        foreach (var chapter in epubBook.ReadingOrder)
        {
            chapters.Add(Chapter.Create(chapter.Content));
        }

        BookContent bookContent = BookContent.Create(
            chapters: chapters,
            css: epubBook.Content.Css.Local.ToDictionary(c => c.Key, c => c.Content),
            images: epubBook.Content.Images.Local.ToDictionary(i => i.Key, i => i.Content)
        );

        Book book = Book.Create(
            title: title,
            cover: cover,
            metadata: bookMetadata,
            content: bookContent,
            publishingInfo: publishingInfo
        );

        book.Authors = authors;

        return book;
    }

    public async Task<Chapter> ReadChapterAsync(string bookFilePath, int chapterIndex)
    {
        var book = await ReadAsync(bookFilePath);
        var index = chapterIndex % book.Metadata.ChaptersCount;
        var chapter = book.Content.Chapters.ElementAt(index);
        var chapterHtml = ProcessHtmlImages(chapter.Html, book.Content.Images);
        return Chapter.Create(chapterHtml);
    }
    
    private string ProcessHtmlImages(string html, Dictionary<string, byte[]> images)
    {
        html = Regex.Replace(html, @"xlink:href=""images/([^""]+)""", match =>
        {
            var imageName = "images/" + match.Groups[1].Value;
            return ReplaceWithBase64(imageName, images, "xlink:href");
        });

        html = Regex.Replace(html, @"(<img[^>]+src\s*=\s*"")images/([^""]+)(""[^>]*>)", match =>
        {
            var imageName = "images/" + match.Groups[2].Value;
            return match.Groups[1].Value + GetBase64Src(imageName, images) + match.Groups[3].Value;
        });

        var matchBody = Regex.Match(html, @"<body.*?>(.*)</body>", RegexOptions.Singleline);
        return matchBody.Success ? matchBody.Groups[1].Value : html;
    }

    private string ReplaceWithBase64(string imageName, Dictionary<string, byte[]> images, string attribute)
    {
        if (images.TryGetValue(imageName, out byte[] imageBytes))
        {
            var base64 = Convert.ToBase64String(imageBytes);
            var ext = GetFileExtension(imageName).TrimStart('.');
            return $"{attribute}=\"data:image/{ext};base64,{base64}\"";
        }
        return $"{attribute}=\"{imageName}\""; 
    }

    private string GetBase64Src(string imageName, Dictionary<string, byte[]> images)
    {
        if (images.TryGetValue(imageName, out byte[] imageBytes))
        {
            var base64 = Convert.ToBase64String(imageBytes);
            var ext = GetFileExtension(imageName).TrimStart('.');
            return $"data:image/{ext};base64,{base64}";
        }
        return imageName;
    }

    private string GetFileExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.TrimStart('.') ?? "";
        return ext;
    }
}