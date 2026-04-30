using Orchid.Application.Models;
using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Common.Services;

public interface ICloudSyncService
{
    Task<SyncResult> TrySyncProgressAsync(
        string providerId,
        ReadingProgress localProgress,
        CancellationToken cancellationToken = default);

    Task<ReadingProgress?> FetchProgressAsync(string providerId, BookId bookId,
        CancellationToken cancellationToken = default);

    Task SaveProgressAsync(string providerId, ReadingProgress progress, CancellationToken cancellationToken = default);
}