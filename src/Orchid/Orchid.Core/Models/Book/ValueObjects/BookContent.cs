namespace Orchid.Core.Models.Book.ValueObjects;

public record BookContent
{
    private BookContent(string plainText, string formattedText)
    {
        PlainText = plainText;
        FormattedText = formattedText;
    }
    public string PlainText { get; private set; }
    public string? FormattedText { get; private set; }

    public static BookContent Create(string plainText, string? formattedText) => new(plainText, formattedText);
}