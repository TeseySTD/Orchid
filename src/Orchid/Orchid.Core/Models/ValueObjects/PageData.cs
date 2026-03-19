using System.Text.Json.Serialization;

namespace Orchid.Core.Models.ValueObjects;

public record PageData
{
    [JsonConstructor]
    private PageData(string html, string locator)
    {
        Html = html;
        Locator = locator;
    }

    public string Locator { get; init; }
    public string Html { get; init; }
    
    public static PageData Create(string html, string locator) => new (html, locator);
}
