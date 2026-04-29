using Orchid.Application.Common.Services;
using Orchid.Core.Models;
using Orchid.Core.Models.ValueObjects;

namespace Orchid.Application.Services;

using System.Text.Json;

public class CloudSyncService : ICloudSyncService
{
    private readonly IEnumerable<ICloudStorageProvider> _providers;

    public CloudSyncService(IEnumerable<ICloudStorageProvider> providers)
    {
        _providers = providers;
    }

    public async Task<ReadingProgress?> FetchProgressAsync(string providerId, BookId bookId,
        CancellationToken cancellationToken = default)
    {
        var provider = GetAuthenticatedProvider(providerId);
        var fileName = GetSyncFileName(bookId);

        var jsonData = await provider.DownloadDataAsync(fileName, cancellationToken);
        if (string.IsNullOrEmpty(jsonData))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ReadingProgress>(jsonData);
    }

    public async Task SaveProgressAsync(string providerId, ReadingProgress progress,
        CancellationToken cancellationToken = default)
    {
        var provider = GetAuthenticatedProvider(providerId);
        var fileName = GetSyncFileName(progress.BookId);

        var jsonData = JsonSerializer.Serialize(progress);
        await provider.UploadDataAsync(fileName, jsonData, cancellationToken);
    }

    private ICloudStorageProvider GetAuthenticatedProvider(string providerId)
    {
        var provider = _providers.FirstOrDefault(p => p.ProviderId == providerId)
                       ?? throw new InvalidOperationException($"Provider {providerId} not found.");

        if (!provider.IsAuthenticated)
        {
            // Requires explicit authentication prior to sync
            throw new UnauthorizedAccessException($"Provider {providerId} is not authenticated.");
        }

        return provider;
    }

    private static string GetSyncFileName(BookId bookId) => $"progress_{bookId.Value}.json";
}