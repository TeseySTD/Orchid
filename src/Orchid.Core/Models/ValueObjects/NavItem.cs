namespace Orchid.Core.Models.ValueObjects;

public class NavItem
{
    private NavItem(string title, int chapterIndex, IEnumerable<NavItem>? nestedNavItems)
    {
        ChapterTitle = title;
        ChapterIndex = chapterIndex;
        NestedItems = nestedNavItems?.ToList() ?? new List<NavItem>();
    }

    public string ChapterTitle { get; init; } = string.Empty;
    public int ChapterIndex { get; init; }
    public List<NavItem> NestedItems { get; init; }

    public static NavItem Create(string title, int chapterIndex, IEnumerable<NavItem>? nestedNavItems) =>
        new(title, chapterIndex, nestedNavItems);

    public static NavItem Create(string title, int chapterIndex) => Create(title, chapterIndex, null);

}