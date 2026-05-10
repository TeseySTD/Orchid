namespace Orchid.Application.Common.Services;

using Models;
using Orchid.Core.Models;

public interface ILibraryService
{
    Task TrackBookAsync(Book book);
    Task<IEnumerable<BookSummary>> GetLibraryBooksAsync();

    Task SaveProgressAsync(ReadingProgress progress);
    Task<ReadingProgress?> GetProgressAsync(string bookId);

    bool IsBookAvailableLocally(string filePath);
}