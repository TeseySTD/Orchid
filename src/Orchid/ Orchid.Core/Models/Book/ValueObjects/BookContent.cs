namespace Orchid.Core.Models.Book.Entities;

public record BookContent
{
    public string PlainText { get; private set; }
    public string FormattedText { get; private set; }
}