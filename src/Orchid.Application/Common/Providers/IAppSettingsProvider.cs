using Orchid.Application.Dto;

namespace Orchid.Application.Common.Providers;

public interface IAppSettingsProvider
{
    bool OpenLastBookOnStartup { get; set; }
    int ReaderFontSize { get; set; }
    double ReaderLineHeight { get; set; }
    AppThemeMode Theme { get; set; }
    string? LastOpenedBookPath { get; set; }
    
    event Action? OnSettingsChanged;
}