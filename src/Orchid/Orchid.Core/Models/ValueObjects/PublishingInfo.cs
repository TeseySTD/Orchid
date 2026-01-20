namespace Orchid.Core.Models.ValueObjects;

public record PublishingInfo
{
    private PublishingInfo(string? publicationDate, List<string> publisher, string? isbn)
    {
        PublicationDate = publicationDate;
        Publisher = publisher;
        ISBN = isbn;
    }

    public string? PublicationDate { get; }
    public List<string> Publisher { get; }
    public string? ISBN { get; }

    public static PublishingInfo Create(string? publicationDate, List<string> publisher, string? isbn)
        => new(publicationDate, publisher, isbn);
    
    public bool IsEmpty() => Publisher.Count == 0 && string.IsNullOrEmpty(ISBN) && PublicationDate == null;
}