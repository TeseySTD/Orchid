namespace Orchid.Core.Models.ValueObjects;

public record CssFile
{
    public string FileName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;

    private CssFile(string filename, string content)
    {
        FileName = filename;
        Content = content;
    }

    public static CssFile Create(string filename, string content) => new (filename, content);
}