using Orchid.Core.Models.ValueObjects;

namespace Orchid.Core.Models;

public class Book
{
    public BookTitle Title { get; private set; }
    public BookMetadata Metadata { get; private set; }
    public List<Author> Authors { get;  set; } = new List<Author>();
    public Cover? Cover { get; private set; }
    public PublishingInfo PublishingInfo { get; private set; }

    private Book(BookTitle title,
        BookMetadata metadata,
        Cover? cover, PublishingInfo publishingInfo)
    {
        Title = title;
        Metadata = metadata;
        Cover = cover;
        PublishingInfo = publishingInfo;
    }

    public static Book Create(BookTitle title,
        BookMetadata metadata,
        Cover? cover, PublishingInfo publishingInfo) => new(title, metadata, cover, publishingInfo);
    
}