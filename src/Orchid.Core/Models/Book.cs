using Orchid.Core.Models.ValueObjects;

namespace Orchid.Core.Models;

public class Book : IEquatable<Book>
{
    public BookId Id { get; }
    public BookTitle Title { get; private set; }
    public BookMetadata Metadata { get; private set; }
    public Navigation Navigation { get; private set; }
    public List<Author> Authors { get; set; } = new List<Author>();
    public Cover? Cover { get; private set; }
    public PublishingInfo PublishingInfo { get; private set; }

    private Book(
        BookId id,
        BookTitle title,
        BookMetadata metadata,
        Cover? cover,
        PublishingInfo publishingInfo,
        Navigation navigation)
    {
        Id = id;
        Title = title;
        Metadata = metadata;
        Cover = cover;
        PublishingInfo = publishingInfo;
        Navigation = navigation;
    }

    public static Book Create(
        BookId id,
        BookTitle title,
        BookMetadata metadata,
        Cover? cover,
        PublishingInfo publishingInfo,
        Navigation navigation
    ) => new(id, title, metadata, cover, publishingInfo, navigation);

    public bool Equals(Book? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Book)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}