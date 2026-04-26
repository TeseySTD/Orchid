namespace Orchid.Core.Models;

public class NavItem
{
    private NavItem(string title, int chapterIndex, IEnumerable<NavItem>? nestedNavItems)
    {
        ChapterTitle = title;
        ChapterIndex = chapterIndex;
        NestedItems = nestedNavItems?.ToList() ?? new List<NavItem>();
    }

    public string ChapterTitle { get; set; } = string.Empty;
    public int ChapterIndex { get; set; }
    public List<NavItem> NestedItems { get; set; }

    public static NavItem Create(string title, int chapterIndex, IEnumerable<NavItem>? nestedNavItems) =>
        new(title, chapterIndex, nestedNavItems);

    public static NavItem Create(string title, int chapterIndex) => Create(title, chapterIndex, null);

}