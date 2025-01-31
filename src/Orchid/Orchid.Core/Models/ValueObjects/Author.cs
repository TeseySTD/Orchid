
namespace Orchid.Core.Models.ValueObjects;

public class Author
{
    private Author(string firstName, string? lastName) => (FirstName, LastName) = (firstName, lastName);

    public string FirstName { get; }
    public string? LastName { get; }

    public static Author Create(string firstName, string? lastName) => new(firstName, lastName);
    public static Author Create(string firstName) => new(firstName, null);

    public override string ToString() => LastName is null ? FirstName : $"{FirstName} {LastName}";
}
