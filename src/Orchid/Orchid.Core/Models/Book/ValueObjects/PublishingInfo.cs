namespace Orchid.Core.Models.Book.ValueObjects;

public record PublishingInfo
{
    private PublishingInfo(DateTime? publicationDate, string? publisher, string? isbn)
    {
        PublicationDate = publicationDate;
        Publisher = publisher;
        ISBN = isbn;
    }

    public DateTime? PublicationDate { get; }
    public string? Publisher { get; }
    public string? ISBN { get; }

    public static PublishingInfo Create(DateTime? publicationDate, string? publisher, string? isbn)
        => new(publicationDate, publisher, isbn);
}