using Orchid.Application.Common.Providers;

namespace Orchid.Presentation.Providers;

public class MauiPlatformEnvironmentProvider : IPlatformEnvironmentProvider
{
    public bool IsDesktop =>
        DeviceInfo.Platform == DevicePlatform.WinUI ||
        DeviceInfo.Platform == DevicePlatform.MacCatalyst;
}