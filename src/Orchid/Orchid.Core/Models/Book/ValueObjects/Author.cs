using Orchid.Core.Common;

namespace Orchid.Core.Models.Book.Entities;

public class Author
{
    private Author(string firstName, string? lastName) => (FirstName, LastName) = (firstName, lastName);

    public string FirstName { get; }
    public string? LastName { get; }

    public static Author Create(string firstName, string? lastName) => new(firstName, lastName);
}
