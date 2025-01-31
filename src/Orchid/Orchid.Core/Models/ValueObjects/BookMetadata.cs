namespace Orchid.Core.Models.ValueObjects;

public record BookMetadata
{
    public string FileName { get; set; } = null!;
    public IEnumerable<Genre> Genres { get; set; }
    public string? Language { get; set; }
    public int ChaptersCount { get; set; }

    public static BookMetadata Create(string? language, int chaptersCount, IEnumerable<Genre> genres, string filename) => new()
    {
        Language = language,
        ChaptersCount = chaptersCount,
        Genres = genres,
        FileName = filename
    };

    public static BookMetadata Create(int chaptersCount, IEnumerable<Genre> genres, string filename) => Create(null, chaptersCount, genres, filename);
}