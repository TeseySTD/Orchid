using Orchid.Application.Common.Providers;

namespace Orchid.Presentation.Providers;

public class MauiWebAuthenticatorProvider : IWebAuthenticatorProvider
{
    public async Task<string?> GetAuthorizationCodeAsync(Uri authorizeUrl, Uri callbackUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(authorizeUrl, callbackUrl);

            return authResult.Properties.TryGetValue("code", out var authorizationCode)
                ? authorizationCode
                : null;
        }
        catch
        {
            return null;
        }
    }
}