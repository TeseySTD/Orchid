using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;
using Orchid.Core.Services;
using Orchid.Engine.Common;
using VersOne.Epub;

namespace Orchid.Engine.Epub;

public class EpubBookService : IBookService
{
    public async Task<Book> ReadAsync(string bookFilePath)
    {
        EpubBookRef epubBook = await EpubReader.OpenBookAsync(bookFilePath);
        BookId bookId;
        await using (var stream = File.OpenRead(bookFilePath))
        {
            bookId = BookIdentityService.GenerateId(stream);
        }

        BookTitle title = BookTitle.Create(epubBook.Title);
        List<Author> authors = epubBook.AuthorList.Select(Author.Create).ToList();
        var coverImage = await epubBook.ReadCoverAsync();
        Cover? cover = Cover.Create("book_cover.png", coverImage);
        string? language = epubBook.Schema.Package.Language;
        Navigation navigation = GetNavigation(epubBook);
        int chaptersCount = navigation.Count;

        BookMetadata bookMetadata = BookMetadata.Create(
            language: language,
            chaptersCount: chaptersCount,
            Path.GetFileName(bookFilePath)
        );

        string? publicationDate;
        if (epubBook.Schema.Package.Metadata.Dates.Count == 1)
            publicationDate = epubBook.Schema.Package.Metadata.Dates[0].Date;
        else
            publicationDate = epubBook.Schema.Package.Metadata.Dates
                .FirstOrDefault(d => d.Event?.ToLower() == "publication")?.Date;

        string? isbn = epubBook.Schema.Package.Metadata.Identifiers
            .FirstOrDefault(i => i.Scheme?.ToLower() == "isbn" || i.Identifier.ToLower().Contains("isbn"))?
            .Identifier;

        PublishingInfo publishingInfo = PublishingInfo.Create(
            publicationDate: publicationDate,
            publishers: epubBook.Schema.Package.Metadata.Publishers
                .Select(p => p.Publisher).ToList(),
            isbn: isbn
        );

        Book book = Book.Create(
            id: bookId,
            title: title,
            cover: cover,
            metadata: bookMetadata,
            publishingInfo: publishingInfo,
            navigation: navigation
        );

        book.Authors = authors;

        return book;
    }

    public async Task<Chapter> ReadChapterAsync(string bookFilePath, int chapterIndex)
    {
        EpubBookRef epubBook = await EpubReader.OpenBookAsync(bookFilePath);

        var chapter = await ReadChapterFromRefAsync(chapterIndex, epubBook);

        return chapter;
    }

    private async Task<Chapter> ReadChapterFromRefAsync(int chapterIndex, EpubBookRef epubBook)
    {
        var readingOrder = await epubBook.GetReadingOrderAsync();
        var navigation = await epubBook.GetNavigationAsync();
        var index = chapterIndex % readingOrder.Count;
        var chapterHtml = await readingOrder[index].ReadContentAsync();
        var chapterKey = readingOrder[index].Key;
        string title;

        if (navigation != null && navigation.Count > 0)
        {
            var navItem = FindNavItemRecursive(navigation, chapterKey);
            if (navItem != null)
                title = navItem.Title;
            else
                title = Chapter.UndefinedTitle;
        }
        else
            title = epubBook.Title;

        return Chapter.Create(title, chapterHtml);
    }

    public async Task<List<Chapter>> ReadChaptersAsync(string bookFilePath)
    {
        EpubBookRef epubBook = await EpubReader.OpenBookAsync(bookFilePath);
        var readingOrder = await epubBook.GetReadingOrderAsync();
        var chapters = new List<Chapter>();

        for (int i = 0; i < readingOrder.Count; i++)
        {
            var chapter = await ReadChapterFromRefAsync(i, epubBook);
            chapters.Add(chapter);
        }

        return chapters;
    }


    private EpubNavigationItemRef? FindNavItemRecursive(IEnumerable<EpubNavigationItemRef> navItems, string chapterKey)
    {
        foreach (var item in navItems)
        {
            if (item.HtmlContentFileRef != null && item.HtmlContentFileRef.Key == chapterKey)
                return item;

            if (item.NestedItems.Any())
            {
                var found = FindNavItemRecursive(item.NestedItems, chapterKey);
                if (found != null)
                    return found;
            }
        }

        return null;
    }

    public async Task<IEnumerable<CssFile>> GetBookCssAsync(string bookFilePath)
    {
        EpubBookRef epubBook = await EpubReader.OpenBookAsync(bookFilePath);
        var raw = epubBook.Content.Css.Local;
        return raw.Select(css => CssFile.Create(css.Key, css.ReadContent()));
    }

    public async IAsyncEnumerable<Image> GetBookImagesAsync(string bookFilePath)
    {
        EpubBookRef epubBook = await EpubReader.OpenBookAsync(bookFilePath);
        foreach (var img in epubBook.Content.Images.Local)
        {
            yield return Image.Create(img.Key, await img.ReadContentAsync());
        }
    }

    private Navigation GetNavigation(EpubBookRef epubBook)
    {
        List<NavItem> navItems = new List<NavItem>();
        var navigation = epubBook.GetNavigation();
        var readingOrder = epubBook.GetReadingOrder();

        if (navigation != null && navigation.Count > 0)
        {
            foreach (var epubNavItem in navigation)
            {
                int chapterIndex = readingOrder.FindIndex(ro => ro.Key == epubNavItem.HtmlContentFileRef!.Key);
                navItems.Add(ConvertEpubNavItemToNavItem(epubNavItem, chapterIndex, readingOrder));
            }
        }
        else
            navItems.Add(NavItem.Create(epubBook.Title, 0));

        return Navigation.Create(navItems);
    }

    private NavItem ConvertEpubNavItemToNavItem(EpubNavigationItemRef epubNavItem, int chapterIndex,
        List<EpubLocalTextContentFileRef> readingOrder)
    {
        List<NavItem> childNavItems = new List<NavItem>();

        if (epubNavItem.NestedItems.Any())
        {
            foreach (var child in epubNavItem.NestedItems)
            {
                int childIndex = readingOrder.FindIndex(ro => ro.Key == child.HtmlContentFileRef!.Key);
                childNavItems.Add(ConvertEpubNavItemToNavItem(child, childIndex, readingOrder));
            }
        }

        return NavItem.Create(epubNavItem.Title, chapterIndex, childNavItems);
    }
}