namespace Orchid.Application.Common.Providers;

public interface IPlatformEnvironmentProvider
{
    bool IsDesktop { get; }
}