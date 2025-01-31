using Orchid.Core.Models.ValueObjects;

namespace Orchid.Core.Models;

public record BookContent
{
    private BookContent(List<Chapter> chapters, Dictionary<string, string> css, Dictionary<string, byte[]> images)
    {
        Chapters = chapters;
        Css = css;
        Images = images;
    }

    public List<Chapter> Chapters { get; set; }
    public Dictionary<string, string> Css { get; set; }
    public Dictionary<string, byte[]> Images { get; set; }

    public static BookContent Create(List<Chapter> chapters, Dictionary<string, string> css, Dictionary<string, byte[]> images) =>
        new(chapters, css, images);
}