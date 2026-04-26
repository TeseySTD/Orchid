namespace Orchid.Core.Models.ValueObjects;

public record BookMetadata
{
    public string FileName { get; set; } = null!;
    public string? Language { get; set; }
    public int ChaptersCount { get; set; }

    public static BookMetadata Create(string? language, int chaptersCount, string filename) => new()
    {
        Language = language,
        ChaptersCount = chaptersCount,
        FileName = filename
    };

    public static BookMetadata Create(int chaptersCount, string filename) => Create(null, chaptersCount, filename);
}