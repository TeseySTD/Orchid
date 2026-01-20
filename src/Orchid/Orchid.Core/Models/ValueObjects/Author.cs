namespace Orchid.Core.Models.ValueObjects;

public class Author
{
    private Author(string name) => Name = name;

    public string Name { get; }

    public static Author Create(string name) => new(name);
}