namespace Orchid.Core.Models.ValueObjects;

public record PublishingInfo
{
    private PublishingInfo(string? publicationDate, string? publisher, string? isbn)
    {
        PublicationDate = publicationDate;
        Publisher = publisher;
        ISBN = isbn;
    }

    public string? PublicationDate { get; }
    public string? Publisher { get; }
    public string? ISBN { get; }

    public static PublishingInfo Create(string? publicationDate, string? publisher, string? isbn)
        => new(publicationDate, publisher, isbn);
}