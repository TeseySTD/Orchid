namespace Orchid.Application.Models;

public record BookSummary(
    string Id,
    string Title,
    string Authors,
    string FilePath,
    string? CoverPath,
    DateTimeOffset LastOpenedAt
);
