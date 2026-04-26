namespace Orchid.Core.Models.ValueObjects;

public record PublishingInfo
{
    private PublishingInfo(string? publicationDate, List<string> publishers, string? isbn)
    {
        PublicationDate = publicationDate;
        Publishers = publishers;
        ISBN = isbn;
    }

    public string? PublicationDate { get; }
    public List<string> Publishers { get; }
    public string? ISBN { get; }

    public static PublishingInfo Create(string? publicationDate, List<string> publishers, string? isbn)
        => new(publicationDate, publishers, isbn);
    
    public bool IsEmpty() => Publishers.Count == 0 && string.IsNullOrEmpty(ISBN) && PublicationDate == null;
}