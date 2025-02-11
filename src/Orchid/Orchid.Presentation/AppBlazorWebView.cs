using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.FileProviders;

namespace Orchid.Presentation;
/// <summary>
/// Custom web view class, that provides access to FileSystem.Current.CacheDirectory.
/// </summary>
public class AppBlazorWebView : BlazorWebView
{
    public override IFileProvider CreateFileProvider(string contentRootDir)
    {
        var lPhysicalFiles = new PhysicalFileProvider(FileSystem.Current.CacheDirectory);
        return new CompositeFileProvider(lPhysicalFiles, base.CreateFileProvider(contentRootDir));
    }
}