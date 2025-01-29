namespace Orchid.Core.Models.Book.ValueObjects;

public record BookId
{
    private BookId(Guid value) => Value = value;
    public Guid Value { get; init; }

    public static BookId Create(Guid value)
    {
        return new BookId(value);
    }
}