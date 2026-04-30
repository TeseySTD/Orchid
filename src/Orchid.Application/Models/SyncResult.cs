namespace Orchid.Application.Models;

using Orchid.Core.Models;

public enum SyncState
{
    Uploaded,
    Downloaded,
    UpToDate,
    ProviderError,
    NetworkError,
    Failed
}

public record SyncResult(
    SyncState State,
    ReadingProgress? ResolvedProgress = null,
    string? ErrorMessage = null
);