using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.FileProviders;
using Orchid.Application.Common.Services;

namespace Orchid.Presentation;

/// <summary>
/// Custom web view class, that provides access to FileSystem.Current.CacheDirectory/>.
/// </summary>
public class AppBlazorWebView : BlazorWebView
{
    public override IFileProvider CreateFileProvider(string contentRootDir)
    {
        var cachePath = FileSystem.Current.CacheDirectory;

        if (!Directory.Exists(cachePath))
        {
            Directory.CreateDirectory(cachePath);
        }

        var cacheFiles = new PhysicalFileProvider(cachePath);
        var appFiles = base.CreateFileProvider(contentRootDir);

        return new CompositeFileProvider(appFiles, cacheFiles);
    }
}