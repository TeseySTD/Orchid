using Orchid.Application.Dto;

namespace Orchid.Application.Common.Services;

using Orchid.Core.Models;

public interface ILibraryService
{
    Task TrackBookAsync(Book book);
    Task DeleteBookAsync(string bookId);
    Task<IEnumerable<BookSummary>> GetLibraryBooksAsync();

    Task SaveProgressAsync(ReadingProgress progress);
    Task<ReadingProgress?> GetProgressAsync(string bookId);

    bool IsBookAvailableLocally(string filePath);
}