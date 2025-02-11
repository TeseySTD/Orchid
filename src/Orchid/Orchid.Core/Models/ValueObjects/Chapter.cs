namespace Orchid.Core.Models.ValueObjects;

public record Chapter
{
    private Chapter(string html)
    {
        Html = html;
    }

    public string Html { get; init; }

    public static Chapter Create(string html) => new (html);
}