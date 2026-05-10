using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Services;

using Orchid.Application.Common.Services;
using Models;
using Orchid.Core.Models;

public class LibraryService(IJsonStorageService jsonStorage) : ILibraryService
{
    private const string SummariesFolder = "library_summaries";
    private const string ProgressFolder = "reading_progress";

    public async Task TrackBookAsync(Book book)
    {
        var key = $"{SummariesFolder}/{book.Id.Value}";

        var summary = new BookSummary(
            Id: book.Id.Value,
            Title: book.Title.Value,
            Authors: string.Join(", ", book.Authors.Select(a => a.Name)),
            FilePath: book.Metadata.FilePath,
            CoverPath: book.Cover?.Path,
            DateTimeOffset.UtcNow
        );

        await jsonStorage.SaveAsync(key, summary);
    }

    public async Task<IEnumerable<BookSummary>> GetLibraryBooksAsync()
    {
        var allSummaries = await jsonStorage.LoadAllInFolderAsync<BookSummary>(SummariesFolder);

        return allSummaries
            .OrderByDescending(b => b.LastOpenedAt)
            .ToList();
    }

    public async Task SaveProgressAsync(ReadingProgress progress)
    {
        var key = $"{ProgressFolder}/{progress.BookId.Value}";
        var dto = new ReadingProgressDto(
            progress.BookId.Value,
            progress.Position.ChapterIndex,
            progress.Position.Locator,
            progress.UpdatedAt);

        await jsonStorage.SaveAsync(key, dto);
    }

    public async Task<ReadingProgress?> GetProgressAsync(string bookId)
    {
        var key = $"{ProgressFolder}/{bookId}";
        var dto = await jsonStorage.LoadAsync<ReadingProgressDto>(key);

        if (dto is null)
            return null;

        var id = BookId.Create(dto.BookId);
        var position = PageIdentifier.Create(dto.ChapterIndex, dto.Position);

        return new ReadingProgress(id, position, dto.UpdatedAt);
    }

    public bool IsBookAvailableLocally(string filePath)
    {
        return File.Exists(filePath);
    }

    private record ReadingProgressDto(string BookId, int ChapterIndex, string Position, DateTimeOffset UpdatedAt);
}