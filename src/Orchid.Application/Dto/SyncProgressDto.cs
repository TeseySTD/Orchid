namespace Orchid.Application.Dto;

public record SyncProgressDto(
    string BookId,
    int ChapterIndex,
    string Locator,
    DateTimeOffset UpdatedAt
);