using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Services;

public interface ICloudSyncService
{
    Task<ReadingProgress?> FetchProgressAsync(string providerId, BookId bookId,
        CancellationToken cancellationToken = default);

    Task SaveProgressAsync(string providerId, ReadingProgress progress, CancellationToken cancellationToken = default);
}