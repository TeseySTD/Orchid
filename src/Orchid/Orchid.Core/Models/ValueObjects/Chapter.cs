namespace Orchid.Core.Models.ValueObjects;

public record Chapter
{
    private Chapter(string html)
    {
        Html = html;
    }

    public string Html { get; set; }

    public static Chapter Create(string html) => new Chapter(html);
}