using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Orchid.Application.Common.Providers;
using Orchid.Application.Dto;
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
    private readonly IWebAuthenticatorProvider _webAuthenticatorProvider;
    private readonly IPlatformEnvironmentProvider _environmentProvider;

    public string ProviderId => "GoogleDrive";
    public string DisplayName => "Google Drive";
    public bool IsAuthenticated => _credential != null && _driveService != null;

    public GoogleDriveProvider(
        string clientId,
        string? clientSecret,
        ISecureStorageProvider secureStorageProvider,
        IWebAuthenticatorProvider webAuthenticatorProvider,
        IPlatformEnvironmentProvider environmentProvider)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _webAuthenticatorProvider = webAuthenticatorProvider;
        _environmentProvider = environmentProvider;
        _tokenStoreAdapter = new OAuthDataStoreAdapter(secureStorageProvider);
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

            if (_environmentProvider.IsDesktop)
            {
                _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets { ClientId = _clientId, ClientSecret = _clientSecret },
                    scopes,
                    "user",
                    cancellationToken,
                    _tokenStoreAdapter);
            }
            else
            {
                string packageName = "io.github.teseystd.orchid";
                string redirectUri = $"{packageName}:/oauth2redirect";
                string requestedScopes = string.Join(" ", scopes);

                string codeVerifier = GenerateCodeVerifier();
                string codeChallenge = GenerateCodeChallenge(codeVerifier);

                var authUrl =
                    new Uri(
                        $"https://accounts.google.com/o/oauth2/v2/auth?client_id={_clientId}&response_type=code&scope={requestedScopes}&redirect_uri={redirectUri}&code_challenge={codeChallenge}&code_challenge_method=S256");
                var callbackUrl = new Uri(redirectUri);

                var authorizationCode =
                    await _webAuthenticatorProvider.GetAuthorizationCodeAsync(authUrl, callbackUrl, cancellationToken);

                if (string.IsNullOrEmpty(authorizationCode))
                {
                    return false;
                }

                var tokenResponse = await ExchangeAuthCodeForTokensAsync(authorizationCode, redirectUri, codeVerifier,
                    cancellationToken);

                if (tokenResponse == null)
                {
                    return false;
                }

                await _tokenStoreAdapter.StoreAsync("user", tokenResponse);

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                        { ClientId = _clientId, ClientSecret = _clientSecret ?? string.Empty },
                    Scopes = scopes,
                    DataStore = _tokenStoreAdapter
                });

                _credential = new UserCredential(flow, "user", tokenResponse);
            }

            _driveService = new DriveService(new BaseClientService.Initializer
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


    private async Task<TokenResponse?> ExchangeAuthCodeForTokensAsync(string code, string redirectUri,
        string codeVerifier, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();

        var requestData = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "code", code },
            { "grant_type", "authorization_code" },
            { "redirect_uri", redirectUri },
            { "code_verifier", codeVerifier }
        };

        if (!string.IsNullOrEmpty(_clientSecret))
        {
            requestData.Add("client_secret", _clientSecret);
        }

        var content = new FormUrlEncodedContent(requestData);
        var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result =
            await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);

        if (result == null || string.IsNullOrEmpty(result.AccessToken))
        {
            return null;
        }

        return new TokenResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresInSeconds = result.ExpiresIn,
            TokenType = result.TokenType,
            IssuedUtc = DateTime.UtcNow 
        };
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
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

            var scopes = new[]
            {
                DriveService.Scope.DriveAppdata,
                "email",
                "profile"
            };

            // Initialize credentials silently without triggering the browser loop
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets =
                    new ClientSecrets { ClientId = _clientId, ClientSecret = _clientSecret ?? string.Empty },
                Scopes = scopes,
                DataStore = _tokenStoreAdapter
            });

            _credential = new UserCredential(flow, "user", token);

            _driveService = new DriveService(new BaseClientService.Initializer
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
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
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


    private class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")] public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")] public long ExpiresIn { get; set; }
    }
}