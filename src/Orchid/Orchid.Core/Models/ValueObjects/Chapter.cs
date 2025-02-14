namespace Orchid.Core.Models.ValueObjects;

public record Chapter
{
    public const string UndefinedTitle = "*";
    private Chapter(string title, string html)
    {
        Html = html;
        Title = title;
    }

    public string Title { get; init; }
    public string Html { get; init; }

    public static Chapter Create(string title, string html) => new (title, html);
}