
namespace Orchid.Core.Models.ValueObjects;

public record Genre
{
    private Genre(string title)
    {
        Title = title;
    }
    public string Title { get; }

    public static Genre Create(string title) => new Genre(title);
}