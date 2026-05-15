using Orchid.Core.Models;

namespace Orchid.Application.Dto;

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