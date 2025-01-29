using Orchid.Core.Common;
using Orchid.Core.Models.Book.Entities;
using Orchid.Core.Models.Book.ValueObjects;

namespace Orchid.Core.Models.Book;

public class Book : AggregateRoot<BookId>
{
    public BookTitle Title { get; private set; }
    public BookMetadata Metadata { get; private set; }
    public BookContent Content { get; private set; }
    public List<Author> Authors { get;  set; }
    public Cover Cover { get; private set; }
    public PublishingInfo PublishingInfo { get; private set; }

    private Book(BookTitle title,
        BookMetadata metadata, BookContent content,
        Cover cover, PublishingInfo publishingInfo) : base(BookId.Create(Guid.NewGuid()))
    {
        Title = title;
        Metadata = metadata;
        Content = content;
        Cover = cover;
        PublishingInfo = publishingInfo;
    }

    public static Book Create(BookTitle title,
        BookMetadata metadata, BookContent content,
        Cover cover, PublishingInfo publishingInfo) => new(title, metadata, content, cover, publishingInfo);
    
}