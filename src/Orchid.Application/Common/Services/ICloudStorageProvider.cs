using Orchid.Application.Models;

namespace Orchid.Application.Common.Services;

public interface ICloudStorageProvider
{
    string ProviderId { get; }
    string DisplayName { get; }
    bool IsAuthenticated { get; }

    Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<bool> TryRestoreSessionAsync(CancellationToken cancellationToken = default);
    Task<CloudUserProfile?> GetUserProfileAsync(CancellationToken cancellationToken = default);
    Task UploadDataAsync(string fileName, string content, CancellationToken cancellationToken = default);
    Task<string?> DownloadDataAsync(string fileName, CancellationToken cancellationToken = default);
}