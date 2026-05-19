using System.Resources;
using System.Globalization;

namespace Orchid.Presentation.Resources.Translations;

public class AppResources
{
    private static readonly ResourceManager ResourceManager =
        new("Orchid.Presentation.Resources.Translations.AppResources", typeof(AppResources).Assembly);

    public static CultureInfo? Culture { get; set; }

    private static string Get(string key) => ResourceManager.GetString(key, Culture) ?? key;

    // UI Strings
    public static string ErrorTitle => Get(nameof(ErrorTitle));
    public static string ErrorSubtitle => Get(nameof(ErrorSubtitle));
    public static string ErrorReloadBtn => Get(nameof(ErrorReloadBtn));
    public static string ErrorGoHomeBtn => Get(nameof(ErrorGoHomeBtn));
    public static string ErrorDetailsPanel => Get(nameof(ErrorDetailsPanel));
    public static string ErrorCopyBtn => Get(nameof(ErrorCopyBtn));
    public static string ErrorInnerException => Get(nameof(ErrorInnerException));
    public static string ErrorCopiedMessage => Get(nameof(ErrorCopiedMessage));
    public static string HomeEmptyTitle => Get(nameof(HomeEmptyTitle));
    public static string HomeEmptySubtitle => Get(nameof(HomeEmptySubtitle));
    public static string HomeAddFirstBookButton => Get(nameof(HomeAddFirstBookButton));
    public static string HomeContinueReading => Get(nameof(HomeContinueReading));
    public static string HomeAllBooks => Get(nameof(HomeAllBooks));
    public static string HomeAddBookFab => Get(nameof(HomeAddBookFab));
    public static string NavMenuLibrary => Get(nameof(NavMenuLibrary));
    public static string NavMenuSettings => Get(nameof(NavMenuSettings));
    public static string NavMenuGitHub => Get(nameof(NavMenuGitHub));
    public static string NavMenuNoProviders => Get(nameof(NavMenuNoProviders));
    public static string NavMenuLogout => Get(nameof(NavMenuLogout));
    public static string NavMenuConnectCloud => Get(nameof(NavMenuConnectCloud));
    public static string NavMenuLoginFailed => Get(nameof(NavMenuLoginFailed));
    public static string NavMenuProfileLoadFailed => Get(nameof(NavMenuProfileLoadFailed));
    public static string NavMenuLogoutFailed => Get(nameof(NavMenuLogoutFailed));
    public static string SettingsTitle => Get(nameof(SettingsTitle));
    public static string SettingsGeneral => Get(nameof(SettingsGeneral));
    public static string SettingsAppearance => Get(nameof(SettingsAppearance));
    public static string SettingsThemeSystem => Get(nameof(SettingsThemeSystem));
    public static string SettingsThemeLight => Get(nameof(SettingsThemeLight));
    public static string SettingsThemeDark => Get(nameof(SettingsThemeDark));
    public static string SettingsStartupTitle => Get(nameof(SettingsStartupTitle));
    public static string SettingsStartupDesc => Get(nameof(SettingsStartupDesc));
    public static string SettingsStorage => Get(nameof(SettingsStorage));
    public static string SettingsPaginationData => Get(nameof(SettingsPaginationData));
    public static string SettingsClearBtn => Get(nameof(SettingsClearBtn));
    public static string SettingsImagesData => Get(nameof(SettingsImagesData));

    public static string SettingsImagesPersistentWarning(string size) =>
        string.Format(Get(nameof(SettingsImagesPersistentWarning)), size);

    public static string SettingsCacheAlert => Get(nameof(SettingsCacheAlert));
    public static string SettingsDialogClearCacheTitle => Get(nameof(SettingsDialogClearCacheTitle));
    public static string SettingsDialogClearPaginationMsg => Get(nameof(SettingsDialogClearPaginationMsg));
    public static string SettingsDialogClearImagesMsg => Get(nameof(SettingsDialogClearImagesMsg));
    public static string SettingsDialogCancel => Get(nameof(SettingsDialogCancel));
    public static string SettingsSnackbarPaginationCleared => Get(nameof(SettingsSnackbarPaginationCleared));
    public static string SettingsSnackbarImagesCleared => Get(nameof(SettingsSnackbarImagesCleared));
    public static string BookLoadingText => Get(nameof(BookLoadingText));
    public static string BookSyncProgressRestored => Get(nameof(BookSyncProgressRestored));
    public static string BookSyncLocalProgressUploaded => Get(nameof(BookSyncLocalProgressUploaded));
    public static string BookSyncCloudNotResponding => Get(nameof(BookSyncCloudNotResponding));
    public static string BookSyncProgressUpdated => Get(nameof(BookSyncProgressUpdated));
    public static string BookMenuChapter => Get(nameof(BookMenuChapter));
    public static string BookMenuPage => Get(nameof(BookMenuPage));
    public static string BookMenuTotal => Get(nameof(BookMenuTotal));
    public static string BookMenuNavigationBtn => Get(nameof(BookMenuNavigationBtn));
    public static string BookSettingsTitle => Get(nameof(BookSettingsTitle));
    public static string BookSettingsFontSize => Get(nameof(BookSettingsFontSize));
    public static string BookSettingsLineHeight => Get(nameof(BookSettingsLineHeight));
    public static string BookSettingsSave => Get(nameof(BookSettingsSave));
    public static string NavigationModalTitle => Get(nameof(NavigationModalTitle));
    public static string NavigationModalPages(int count) =>
        string.Format(Get(nameof(NavigationModalPages)), count);
    public static string DeleteBookConfirmTitle => Get(nameof(DeleteBookConfirmTitle));
    public static string DeleteBookConfirmMessage => Get(nameof(DeleteBookConfirmMessage));
    public static string Delete => Get(nameof(Delete));
    public static string Cancel => Get(nameof(Cancel));
}