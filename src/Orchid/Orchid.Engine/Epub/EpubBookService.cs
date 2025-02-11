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
        Cover? cover = Cover.Create("book_cover.png", epubBook.CoverImage);
        string? language = epubBook.Schema.Package.Language;
        int chaptersCount = epubBook.ReadingOrder.Count;

        BookMetadata bookMetadata = BookMetadata.Create(
            language: language,
            chaptersCount: chaptersCount,
            Path.GetFileName(bookFilePath)
        );

        PublishingInfo publishingInfo = PublishingInfo.Create(
            publicationDate: epubBook.Schema.Package.Metadata.Dates.Count > 0
                ? epubBook.Schema.Package.Metadata.Dates[0].Date
                : null,
            publisher: epubBook.Schema.Package.Metadata.Publishers.Count > 0
                ? epubBook.Schema.Package.Metadata.Publishers[0].Publisher
                : null,
            null
        );

        Book book = Book.Create(
            title: title,
            cover: cover,
            metadata: bookMetadata,
            publishingInfo: publishingInfo
        );

        book.Authors = authors;

        return book;
    }

    public async Task<Chapter> ReadChapterAsync(string bookFilePath, int chapterIndex)
    {
        EpubBook epubBook = await EpubReader.ReadBookAsync(bookFilePath);
        
        var index = chapterIndex % epubBook.ReadingOrder.Count;
        var chapterHtml = epubBook.ReadingOrder.ElementAt(index).Content;
        
        return Chapter.Create(chapterHtml);
    }

    public async Task<IEnumerable<CssFile>> GetBookCssAsync(string bookFilePath)
    {
        EpubBook epubBook = await EpubReader.ReadBookAsync(bookFilePath);
        var raw = epubBook.Content.Css.Local;
        return raw.Select(css => CssFile.Create(css.Key, css.Content));
    }

    public async Task<IEnumerable<Image>> GetBookImagesAsync(string bookFilePath)
    {
        EpubBook epubBook = await EpubReader.ReadBookAsync(bookFilePath);
        var raw = epubBook.Content.Images.Local;
        return raw.Select(css => Image.Create(css.Key, css.Content));
    }
}