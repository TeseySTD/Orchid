namespace Orchid.Application.Models;

public record SyncProgressDto(
    string BookId,
    int ChapterIndex,
    string Locator,
    DateTimeOffset UpdatedAt
);