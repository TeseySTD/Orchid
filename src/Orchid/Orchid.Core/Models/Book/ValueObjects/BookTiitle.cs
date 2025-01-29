namespace Orchid.Core.Models.Book.ValueObjects;

public record BookTitle
{
    private BookTitle(string value) => Value = value;

    public string Value { get; }

    public static BookTitle Create(string value) => 
        string.IsNullOrWhiteSpace(value) 
            ? throw new ArgumentException("Title cannot be empty") 
            : new BookTitle(value);
};