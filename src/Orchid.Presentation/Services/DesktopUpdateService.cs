#if WINDOWS
using Velopack;
using Velopack.Sources;

namespace Orchid.Presentation.Services;

public class DesktopUpdateService
{
    private readonly string _githubRepositoryUrl = "https://github.com/TeseySTD/Orchid";

    public async Task CheckAndApplyUpdatesInBackgroundAsync()
    {
        try
        {
            var updateManager = new UpdateManager(new GithubSource(_githubRepositoryUrl, null, false));
            if (!updateManager.IsInstalled)
            {
                return;
            }

            var newUpdateInfo = await updateManager.CheckForUpdatesAsync();
            if (newUpdateInfo == null)
            {
                return;
            }

            await updateManager.DownloadUpdatesAsync(newUpdateInfo);
            
            updateManager.ApplyUpdatesAndRestart(newUpdateInfo);
        }
        catch
        {
            // Fail silently to prevent app crashes during network or API issues
        }
    }
}
#endif
