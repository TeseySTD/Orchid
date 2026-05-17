namespace Orchid.Application.Common.Providers;

public interface IWebAuthenticatorProvider
{
    Task<string?> GetAuthorizationCodeAsync(Uri authorizeUrl, Uri callbackUrl,
        CancellationToken cancellationToken = default);
}