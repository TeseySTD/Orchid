namespace Orchid.Core.Models.ValueObjects;

public record BookMetadata
{
    public string FilePath { get; init; } = null!;
    public string? Language { get; init; }
    public int ChaptersCount { get; init; }

    public static BookMetadata Create(string? language, int chaptersCount, string filepath) => new()
    {
        Language = language,
        ChaptersCount = chaptersCount,
        FilePath = filepath
    };

    public static BookMetadata Create(int chaptersCount, string filepath) => Create(null, chaptersCount, filepath);
}