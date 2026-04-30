using Orchid.Application.Common.Services;
using Orchid.Application.Models;
using System.Net.Sockets;
using System.Text.Json;
using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Services;

public class CloudSyncService : ICloudSyncService
{
    private readonly IEnumerable<ICloudStorageProvider> _providers;

    public CloudSyncService(IEnumerable<ICloudStorageProvider> providers)
    {
        _providers = providers;
    }

    public async Task<SyncResult> TrySyncProgressAsync(
        string providerId,
        ReadingProgress localProgress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = _providers.FirstOrDefault(p => p.ProviderId == providerId);

            if (provider is null || !provider.IsAuthenticated)
            {
                return new SyncResult(SyncState.ProviderError, null,
                    $"Provider {providerId} is missing or not authenticated.");
            }

            var remoteProgress = await FetchProgressInternalAsync(provider, localProgress.BookId, cancellationToken);

            if (remoteProgress is null || localProgress.UpdatedAt > remoteProgress.UpdatedAt)
            {
                await SaveProgressInternalAsync(provider, localProgress, cancellationToken);
                return new SyncResult(SyncState.Uploaded, localProgress);
            }

            if (remoteProgress.UpdatedAt > localProgress.UpdatedAt)
            {
                return new SyncResult(SyncState.Downloaded, remoteProgress);
            }

            return new SyncResult(SyncState.UpToDate, localProgress);
        }
        catch (Exception ex) when (IsNetworkError(ex))
        {
            return new SyncResult(SyncState.NetworkError, null, ex.Message);
        }
        catch (Exception ex)
        {
            return new SyncResult(SyncState.Failed, null, ex.Message);
        }
    }

    public async Task<ReadingProgress?> FetchProgressAsync(
        string providerId,
        BookId bookId,
        CancellationToken cancellationToken = default)
    {
        var provider = GetValidProvider(providerId);
        return await FetchProgressInternalAsync(provider, bookId, cancellationToken);
    }

    public async Task SaveProgressAsync(
        string providerId,
        ReadingProgress progress,
        CancellationToken cancellationToken = default)
    {
        var provider = GetValidProvider(providerId);
        await SaveProgressInternalAsync(provider, progress, cancellationToken);
    }

    private async Task<ReadingProgress?> FetchProgressInternalAsync(
        ICloudStorageProvider provider,
        BookId bookId,
        CancellationToken cancellationToken)
    {
        var fileName = GetSyncFileName(bookId);
        var jsonData = await provider.DownloadDataAsync(fileName, cancellationToken);

        if (string.IsNullOrEmpty(jsonData))
        {
            return null;
        }
        
        try
        {
            var dto = JsonSerializer.Deserialize<SyncProgressDto>(jsonData);
            if (dto is null)
            {
                return null;
            }

            return new ReadingProgress(
                BookId.Create(dto.BookId),
                PageIdentifier.Create(dto.ChapterIndex, dto.Locator),
                dto.UpdatedAt);
        }
        catch (JsonException)
        {
            // Fallback for older JSON schema versions or corrupted data
            return null;
        }
    }

    private async Task SaveProgressInternalAsync(
        ICloudStorageProvider provider,
        ReadingProgress progress,
        CancellationToken cancellationToken)
    {
        var dto = new SyncProgressDto(
            progress.BookId.Value,
            progress.Position.ChapterIndex,
            progress.Position.Locator,
            progress.UpdatedAt);

        var fileName = GetSyncFileName(progress.BookId);
        var jsonData = JsonSerializer.Serialize(dto);

        await provider.UploadDataAsync(fileName, jsonData, cancellationToken);
    }

    private ICloudStorageProvider GetValidProvider(string providerId)
    {
        var provider = _providers.FirstOrDefault(p => p.ProviderId == providerId)
                       ?? throw new InvalidOperationException($"Provider {providerId} not found.");

        if (!provider.IsAuthenticated)
        {
            throw new UnauthorizedAccessException($"Provider {providerId} is not authenticated.");
        }

        return provider;
    }

    private static string GetSyncFileName(BookId bookId) => $"progress_{bookId.Value}.json";

    private static bool IsNetworkError(Exception ex)
    {
        var baseException = ex.GetBaseException();

        return ex is OperationCanceledException
               || baseException is SocketException
               || baseException is IOException
               || baseException is HttpRequestException;
    }
}