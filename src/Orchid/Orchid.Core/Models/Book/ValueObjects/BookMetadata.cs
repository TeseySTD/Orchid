using Orchid.Core.Models.Book.ValueObjects;

namespace Orchid.Core.Models.Book.Entities;

public record BookMetadata
{
    public IEnumerable<Genre> Genres { get; set; }
    public string? Language { get; set; }
    public int PageCount { get; set; }

    public static BookMetadata Create(string? language, int pageCount, IEnumerable<Genre> genres) => new()
    {
        Language = language,
        PageCount = pageCount,
        Genres = genres
    };

    public static BookMetadata Create(int pageCount, IEnumerable<Genre> genres) => Create(null, pageCount, genres);
}