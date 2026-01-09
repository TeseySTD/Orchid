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
        Navigation navigation = GetNavigation(epubBook);
        int chaptersCount = navigation.Count;

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
            publishingInfo: publishingInfo,
            navigation: navigation
        );

        book.Authors = authors;

        return book;
    }

    public async Task<Chapter> ReadChapterAsync(string bookFilePath, int chapterIndex)
    {
        EpubBook epubBook = await EpubReader.ReadBookAsync(bookFilePath);

        var chapter = ReadChapter(chapterIndex, epubBook);

        return chapter;
    }

    private Chapter ReadChapter(int chapterIndex, EpubBook epubBook)
    {
        var index = chapterIndex % epubBook.ReadingOrder.Count;
        var chapterHtml = epubBook.ReadingOrder[index].Content;
        var chapterKey = epubBook.ReadingOrder[index].Key;
        string title;

        if (epubBook.Navigation != null && epubBook.Navigation.Count > 0)
        {
            var navItem = FindNavItemRecursive(epubBook.Navigation, chapterKey);
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
        EpubBook epubBook = await EpubReader.ReadBookAsync(bookFilePath);
        var chapters = new List<Chapter>();

        for (int i = 0; i < epubBook.ReadingOrder.Count; i++)
        {
            var chapter = ReadChapter(i, epubBook);
            chapters.Add(chapter);
        }
        return chapters;
    }


    private EpubNavigationItem? FindNavItemRecursive(IEnumerable<EpubNavigationItem> navItems, string chapterKey)
    {
        foreach (var item in navItems)
        {
            if (item.HtmlContentFile != null && item.HtmlContentFile.Key == chapterKey)
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
        EpubBook epubBook = await EpubReader.ReadBookAsync(bookFilePath);
        var raw = epubBook.Content.Css.Local;
        return raw.Select(css => CssFile.Create(css.Key, css.Content));
    }

    public async Task<IEnumerable<Image>> GetBookImagesAsync(string bookFilePath)
    {
        EpubBook epubBook = await EpubReader.ReadBookAsync(bookFilePath);
        var raw = epubBook.Content.Images.Local;
        return raw.Select(img => Image.Create(img.Key, img.Content));
    }

    private Navigation GetNavigation(EpubBook epubBook)
    {
        List<NavItem> navItems = new List<NavItem>();

        if (epubBook.Navigation != null && epubBook.Navigation.Count > 0)
        {
            foreach (var epubNavItem in epubBook.Navigation)
            {
                int chapterIndex = epubBook.ReadingOrder.FindIndex(ro => ro.Key == epubNavItem.HtmlContentFile!.Key);
                navItems.Add(ConvertEpubNavItemToNavItem(epubNavItem, chapterIndex, epubBook));
            }
        }
        else
            navItems.Add(NavItem.Create(epubBook.Title, 0));

        return Navigation.Create(navItems);
    }

    private NavItem ConvertEpubNavItemToNavItem(EpubNavigationItem epubNavItem, int chapterIndex, EpubBook epubBook)
    {
        List<NavItem> childNavItems = new List<NavItem>();

        if (epubNavItem.NestedItems.Any())
        {
            foreach (var child in epubNavItem.NestedItems)
            {
                int childIndex = epubBook.ReadingOrder.FindIndex(ro => ro.Key == child.HtmlContentFile!.Key);
                childNavItems.Add(ConvertEpubNavItemToNavItem(child, childIndex, epubBook));
            }
        }

        return NavItem.Create(epubNavItem.Title, chapterIndex, childNavItems);
    }
}