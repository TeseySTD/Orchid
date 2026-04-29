namespace Orchid.Infrastructure.Cloud.Options;

public class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    public string AndroidClientId { get; init; } = string.Empty;
    public string IosClientId { get; init; } = string.Empty;
    public string DesktopClientId { get; init; } = string.Empty;
    public string DesktopClientSecret { get; init; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; } = string.Empty;
}