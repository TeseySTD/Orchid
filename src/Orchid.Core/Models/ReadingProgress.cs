namespace Orchid.Core.Models;

using ValueObjects;

public class ReadingProgress
{
    public BookId BookId { get; init; }
    public PageIdentifier Position { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public ReadingProgress(BookId bookId, PageIdentifier position, DateTimeOffset updatedAt)
    {
        BookId = bookId;
        Position = position;
        UpdatedAt = updatedAt;
    }

    public void UpdatePosition(PageIdentifier newPosition)
    {
        if (Position == newPosition) return;

        Position = newPosition;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}