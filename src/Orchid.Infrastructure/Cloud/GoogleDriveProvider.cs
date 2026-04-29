using System.Text;
using Orchid.Application.Common.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Orchid.Application.Models;
using Orchid.Infrastructure.Cloud.Auth;

namespace Orchid.Infrastructure.Cloud;

public class GoogleDriveProvider : ICloudStorageProvider
{
    private const string AppDataFolder = "appDataFolder";
    private readonly string _clientId;
    private readonly string? _clientSecret;
    private UserCredential? _credential;
    private DriveService? _driveService;
    private readonly IDataStore _tokenStoreAdapter;

    public string ProviderId => "GoogleDrive";
    public string DisplayName => "Google Drive";
    public bool IsAuthenticated => _credential != null && _driveService != null;

    public GoogleDriveProvider(
        string clientId,
        string? clientSecret,
        ILocalSecureStorage secureStorage)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _tokenStoreAdapter = new OAuthDataStoreAdapter(secureStorage);
    }

    public async Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var scopes = new[]
            {
                DriveService.Scope.DriveAppdata,
                "email",
                "profile"
            };

            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = _clientId, ClientSecret = _clientSecret },
                scopes,
                "user",
                cancellationToken,
                _tokenStoreAdapter);

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "Orchid"
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _tokenStoreAdapter.GetAsync<TokenResponse>("user");

            if (token == null || string.IsNullOrEmpty(token.AccessToken))
            {
                return false;
            }

            return await AuthenticateAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    public async Task<CloudUserProfile?> GetUserProfileAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated || _driveService == null) return null;

        try
        {
            var request = _driveService.About.Get();

            // Limit payload size to required fields only
            request.Fields = "user(displayName, emailAddress, photoLink)";

            var about = await request.ExecuteAsync(cancellationToken);
            var user = about.User;

            if (user == null) return null;

            return new CloudUserProfile(
                user.DisplayName ?? "Unknown User",
                user.EmailAddress,
                user.PhotoLink
            );
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (_credential != null)
        {
            await _credential.RevokeTokenAsync(cancellationToken);
            _credential = null;
            _driveService = null;
        }
    }

    public async Task UploadDataAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated) throw new UnauthorizedAccessException();

        var existingFileId = await GetFileIdAsync(fileName, cancellationToken);
        var byteArray = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(byteArray);

        if (existingFileId != null)
        {
            var updateRequest = _driveService!.Files.Update(new Google.Apis.Drive.v3.Data.File(), existingFileId,
                stream, "application/json");
            await updateRequest.UploadAsync(cancellationToken);
        }
        else
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { AppDataFolder }
            };

            var createRequest = _driveService!.Files.Create(fileMetadata, stream, "application/json");
            await createRequest.UploadAsync(cancellationToken);
        }
    }

    public async Task<string?> DownloadDataAsync(string fileName, CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated) throw new UnauthorizedAccessException();

        var fileId = await GetFileIdAsync(fileName, cancellationToken);
        if (fileId == null) return null;

        var request = _driveService!.Files.Get(fileId);
        using var stream = new MemoryStream();
        await request.DownloadAsync(stream, cancellationToken);

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private async Task<string?> GetFileIdAsync(string fileName, CancellationToken cancellationToken)
    {
        var request = _driveService!.Files.List();
        request.Spaces = AppDataFolder;
        request.Fields = "files(id, name)";
        request.Q = $"name='{fileName}'";

        var result = await request.ExecuteAsync(cancellationToken);
        return result.Files?.FirstOrDefault()?.Id;
    }
}