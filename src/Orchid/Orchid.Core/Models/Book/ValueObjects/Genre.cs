using Orchid.Core.Common;

namespace Orchid.Core.Models.Book.ValueObjects;

public record Genre
{
    private Genre(string title)
    {
        Title = title;
    }
    public string Title { get; }

    public static Genre Create(string title) => new Genre(title);
}