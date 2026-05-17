namespace Orchid.Presentation;

using Android.App;
using Android.Content;
using Android.Content.PM;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter([Intent.ActionView],
    Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = "io.github.teseystd.orchid",
    DataPath = "/oauth2redirect")]
public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
{
}