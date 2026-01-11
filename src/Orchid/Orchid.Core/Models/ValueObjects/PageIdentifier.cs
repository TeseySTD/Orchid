namespace Orchid.Core.Models.ValueObjects;

public record PageIdentifier
{
    private PageIdentifier(int chapterIndex, string locator) => (ChapterIndex, Locator) = (chapterIndex, locator);
    public int ChapterIndex { get; init; }
    public string Locator { get; init; } = string.Empty;

    public static PageIdentifier Empty => new(0, string.Empty);

    public static PageIdentifier Create(int chapterIndex, string locator)
    {
        if (string.IsNullOrEmpty(locator) || chapterIndex < 0)
            return Empty;
        return new PageIdentifier(chapterIndex, locator);
    }
}