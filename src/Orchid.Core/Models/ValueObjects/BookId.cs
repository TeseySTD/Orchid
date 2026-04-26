namespace Orchid.Core.Models.ValueObjects;

public record BookId
{
    public string Value { get; init; }
    
    private BookId(string value) => Value = value;

    public static BookId Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value), $"{nameof(value)} cannot be null or whitespace.");
        return new (value);
    }
    
}